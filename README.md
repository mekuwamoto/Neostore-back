# Neostore — Back-end

API administrativa REST para gestão de catálogo do Neostore. Construída em ASP.NET Core 10 seguindo Clean Architecture e CQRS.

## Stack

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 10 / ASP.NET Core |
| ORM | EF Core 9 + Pomelo (MariaDB) |
| CQRS | MediatR 11 |
| Validação | FluentValidation 11 |
| Mapeamento | AutoMapper 16 |
| Logging | Serilog (Console + File) |
| Docs | OpenAPI + Scalar (`/scalar/v1`) |
| Testes | xUnit + Moq + AwesomeAssertions |
| Infra local | Docker Compose (MariaDB) |

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

## Quick Start

```bash
# 1. Subir banco de dados local
docker compose up -d

# 2. Restaurar dependências
cd src
dotnet restore

# 3. Aplicar migrations
dotnet ef database update --project Neostore.Persistence --startup-project Neostore.Api

# 4. Rodar a API
dotnet run --project Neostore.Api
```

API disponível em `http://localhost:5085`.
Documentação interativa em `http://localhost:5085/scalar/v1`.

## Estrutura do Projeto

```
src/
  Neostore.Api/           # Controllers, middlewares, DI, configuração
  Neostore.Application/   # CQRS: commands, queries, handlers, validators, DTOs
  Neostore.Domain/        # Entidades e regras de negócio (sem dependências externas)
  Neostore.Infrastructure/# Serviços externos e cross-cutting concerns
  Neostore.Persistence/   # EF Core DbContext, configurações, repositórios, migrations
  Neostore.Tests/         # Testes unitários
```

**Dependências (sentido único):**
```
Api → Application → Domain
Infrastructure → Domain
Persistence → Domain
```

## Endpoints

### Categorias — `/api/admin/categorias`

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/` | Criar categoria |
| `GET` | `/` | Listar todas (árvore hierárquica) |
| `GET` | `/{id}` | Buscar por ID |
| `PUT` | `/{id}` | Atualizar categoria |
| `DELETE` | `/{id}` | Excluir (bloqueado se houver produtos ou subcategorias) |

### Produtos — `/api/admin/produtos`

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/` | Criar produto |
| `GET` | `/` | Listagem paginada (filtros: `nome`, `sku`, `idCategoria`) |
| `GET` | `/{id}` | Buscar por ID |
| `PUT` | `/{id}` | Atualizar produto |
| `PATCH` | `/{id}/estoque` | Ajustar estoque (delta, nunca abaixo de zero) |
| `DELETE` | `/{id}` | Soft-delete |

## Comandos

Todos executados a partir de `src/`:

```bash
dotnet build                                    # Build da solução
dotnet test                                     # Todos os testes
dotnet test --filter Name=<TestMethod>          # Teste específico
dotnet run --project Neostore.Api               # Rodar a API
```

## Configuração

As configurações por ambiente ficam em `appsettings.json` (base) e `appsettings.Development.json` (override local).

| Seção | Descrição |
|---|---|
| `Database` | Host, porta, nome do banco, usuário e senha |
| `Cors.AllowedOrigins` | Origins permitidas (ex: `http://localhost:4200` em dev) |
| `Serilog` | Níveis de log e sinks (Console + File) |

## Banco de Dados

MariaDB 10.6 via Docker Compose. Credenciais de desenvolvimento:

| Parâmetro | Valor |
|---|---|
| Host | `localhost:3306` |
| Banco | `neostore` |
| Usuário | `neo` / senha `neopass` |
| Root | `root` / senha `example` |

## Testes

```bash
cd src
dotnet test
```

Cobertura coletada via Coverlet. Testes cobrem handlers, validators e entidades de domínio. Não use `Assert` do xUnit — use AwesomeAssertions.

## Arquitetura de Decisões (ADRs)

Documentação técnica em `specs/`:

| ADR | Decisão |
|---|---|
| [00](specs/00-Introduçao.md) | Escopo inicial — API admin de catálogo |
| [01](specs/01-Definição%20esquemas%20adicionais%20sistema.md) | CQRS, MediatR e entidades de domínio |
| [02](specs/02-Implementação%20cruds.md) | Status de implementação dos CRUDs |
| [03](specs/03-soft-delete.md) | Soft-delete em Produto e UsuarioAdmin |
| [04](specs/04-middleware.md) | Serilog + ExceptionMiddleware + LoggingBehavior |
| [05](specs/05-swagger.md) | OpenAPI com Scalar UI |
| [06](specs/06-automapper.md) | AutoMapper para mapeamento de DTOs |
| [07](specs/07-crud-usuario-admin.md) | CRUD de Usuários Admin com BCrypt |
| [08](specs/08-migrations.md) | EF Core Migrations |
| [09](specs/09-integracao-front-back.md) | CORS para integração com Angular |
