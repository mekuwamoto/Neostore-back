# ADR-08: Migration Inicial — EF Core Code-First

## Status
Implemented

## Date
2026-05-10

## Context

O projeto usa EF Core 9 com Pomelo (MySQL). Até agora o schema era criado via `EnsureCreated()` ou manualmente. Para ambientes de CI/CD e produção é necessário um sistema de migrations versionado.

Quatro entidades mapeadas em `NeostoreDbContext`: `Categoria`, `Produto`, `Imagem`, `UsuarioAdmin`. Todas as configurações estão em Fluent API (`ApplyConfigurationsFromAssembly`); nenhuma `DataAnnotation` no domínio.

## Decision

Adotar EF Core Migrations (`dotnet ef migrations add`) como mecanismo oficial de evolução de schema. A migration inicial (`InitialCreate`) captura o estado atual completo de todas as tabelas.

Configuração do banco via **Options Pattern** — as credenciais são definidas como objeto estruturado em `appsettings.json` e lidas pelo `DatabaseOptions` record, eliminando connection strings literais no código.

---

## Schema Alvo — Migration Inicial

### Tabela `categorias`

| Coluna          | Tipo          | Constraints                        |
|-----------------|---------------|------------------------------------|
| `id`            | `char(36)`    | PK, not null                       |
| `nome`          | `varchar(255)` | not null                          |
| `slug`          | `varchar(255)` | not null                          |
| `id_categoria_pai` | `char(36)` | nullable, FK → `categorias(id)` RESTRICT |

**Indexes:**
- `IX_categorias_nome` — unique
- `IX_categorias_slug` — unique
- `IX_categorias_id_categoria_pai` — FK index

---

### Tabela `produtos`

| Coluna        | Tipo            | Constraints                           |
|---------------|-----------------|---------------------------------------|
| `id`          | `char(36)`      | PK, not null                          |
| `nome`        | `varchar(255)`  | not null                              |
| `sku`         | `varchar(50)`   | not null                              |
| `preco`       | `decimal(18,2)` | not null                              |
| `id_categoria`| `char(36)`      | not null, FK → `categorias(id)` RESTRICT |
| `descricao`   | `varchar(1000)` | nullable                              |
| `estoque`     | `int`           | not null                              |
| `ativo`       | `tinyint(1)`    | not null, default `1`                 |
| `deletado_em` | `datetime(6)`   | nullable                              |

**Indexes:**
- `IX_produtos_sku` — unique
- `IX_produtos_id_categoria` — FK index

**Global Query Filter:** `p => p.Ativo` (exclui soft-deleted por padrão)

---

### Tabela `imagens`

| Coluna          | Tipo           | Constraints                          |
|-----------------|----------------|--------------------------------------|
| `id`            | `char(36)`     | PK, not null                         |
| `nome_arquivo`  | `varchar(255)` | not null                             |
| `chave_s3`      | `varchar(500)` | not null                             |
| `tipo_conteudo` | `varchar(100)` | nullable                             |
| `tamanho_bytes` | `bigint`       | not null                             |
| `id_produto`    | `char(36)`     | not null, FK → `produtos(id)` CASCADE |
| `data_criacao`  | `datetime(6)`  | not null                             |

**Indexes:**
- `IX_imagens_id_produto` — non-unique (FK index)

---

### Tabela `usuarios_admin`

| Coluna        | Tipo           | Constraints                 |
|---------------|----------------|-----------------------------|
| `id`          | `char(36)`     | PK, not null                |
| `email`       | `varchar(255)` | not null                    |
| `senha_hash`  | `varchar(500)` | not null                    |
| `role`        | `varchar(50)`  | not null                    |
| `ativo`       | `tinyint(1)`   | not null, default `1`       |
| `deletado_em` | `datetime(6)`  | nullable                    |

**Indexes:**
- `IX_usuarios_admin_email` — unique

**Global Query Filter:** `u => u.Ativo`

---

## Comandos

Todos os comandos rodam a partir de `src/`:

```bash
# Gerar migration inicial
dotnet ef migrations add InitialCreate \
  --project Neostore.Persistence \
  --startup-project Neostore.Api \
  --output-dir Migrations

# Aplicar ao banco
dotnet ef database update \
  --project Neostore.Persistence \
  --startup-project Neostore.Api

# Reverter (rollback para estado vazio)
dotnet ef database update 0 \
  --project Neostore.Persistence \
  --startup-project Neostore.Api
```

---

## Options Pattern — Configuração do Banco

As credenciais são definidas em `appsettings.json` como objeto estruturado:

```json
{
  "Database": {
    "Server": "localhost",
    "Port": 3306,
    "Database": "neostore",
    "User": "root",
    "Password": ""
  }
}
```

A classe `DatabaseOptions` (`Neostore.Persistence/Options/DatabaseOptions.cs`) mapeia a seção e expõe `ToConnectionString()`:

```csharp
public class DatabaseOptions
{
    public const string SectionName = "Database";
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; } = 3306;
    public string Database { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public string ToConnectionString() =>
        $"Server={Server};Port={Port};Database={Database};User={User};Password={Password};";
}
```

`AddPersistence()` em `DependencyInjection.cs` registra as options e constrói a connection string via interpolação:

```csharp
services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
DatabaseOptions dbOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()!;
string connectionString = dbOptions.ToConnectionString();
services.AddDbContext<NeostoreDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
```

Para sobrescrever em produção/CI, use variáveis de ambiente com separador `__`:

```
Database__Server=db.prod.internal
Database__Password=s3cr3t
```

---

## Pré-requisitos

1. `dotnet-ef` instalado globalmente:
   ```bash
   dotnet tool install --global dotnet-ef
   ```
2. Sem `EnsureCreated()` no `DbContext` — incompatível com migrations.
3. Credenciais válidas em `appsettings.json` (ou variáveis de ambiente) com MySQL rodando para `database update`.

### DesignTimeDbContextFactory

`NeostoreDbContextFactory` (`Neostore.Persistence/Context/`) lê `appsettings.json` da pasta `Neostore.Api` via `ConfigurationBuilder`. Permite rodar `dotnet ef` sem banco ativo (usa `MySqlServerVersion(8, 0, 0)` fixo em vez de `AutoDetect`).

---

## Ordem de criação das tabelas (dependências FK)

1. `categorias` (auto-referenciante — FK é nullable, sem dependência externa)
2. `usuarios_admin` (independente)
3. `produtos` (depende de `categorias`)
4. `imagens` (depende de `produtos`)

EF Core resolve essa ordem automaticamente na migration gerada.

---

## Soft Delete — Observações

`Produto` e `UsuarioAdmin` usam soft delete via campo `deletado_em` + `ativo`. O `HasQueryFilter` filtra registros inativos globalmente. Para queries que precisam ignorar o filtro (ex.: verificar duplicidade de e-mail em usuários deletados), usar `.IgnoreQueryFilters()`.

---

## Migrações Futuras Previstas

| Migration | Trigger |
|-----------|---------|
| `AddJwtRefreshToken` | Implementação de autenticação JWT (ADR futuro) |
| `AddProdutoSlug` | Se slug for adicionado a Produto |
| `SeedUsuarioAdminPadrao` | Seed do usuário admin inicial |
