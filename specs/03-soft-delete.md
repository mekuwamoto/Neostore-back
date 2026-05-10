# Plano de Implementação: Soft Delete

Este documento detalha o roteiro técnico para implementar soft delete nas entidades do sistema, substituindo a exclusão física (hard delete) por exclusão lógica (marcar como inativo).

## Escopo

| Entidade | Soft Delete | Justificativa |
| :------- | :---------- | :------------ |
| `Produto` | ✅ Sim | Preservar histórico de pedidos futuros |
| `Categoria` | ❌ Não | Exclusão já bloqueada por regras de negócio (produtos/subcategorias) |
| `UsuarioAdmin` | ✅ Sim | Preservar histórico de auditoria |

---

## Passo 1 — Domínio (`Neostore.Domain`)

### 1.1 Adicionar interface `ISoftDeletable`

Criar arquivo `Neostore.Domain/Interfaces/ISoftDeletable.cs`:

```csharp
public interface ISoftDeletable
{
    bool Ativo { get; set; }
    DateTime? DeletadoEm { get; set; }
}
```

### 1.2 Implementar `ISoftDeletable` nas entidades

**`Produto.cs`** — adicionar propriedades:
```csharp
public bool Ativo { get; set; } = true;
public DateTime? DeletadoEm { get; set; }
```

**`UsuarioAdmin.cs`** — adicionar propriedades:
```csharp
public bool Ativo { get; set; } = true;
public DateTime? DeletadoEm { get; set; }
```

---

## Passo 2 — Persistência (`Neostore.Persistence`)

### 2.1 Atualizar Fluent API Configurations

**`ProdutoConfiguration.cs`** — adicionar mapeamento:
```csharp
builder.Property(p => p.Ativo)
    .HasColumnName("ativo")
    .IsRequired()
    .HasDefaultValue(true);

builder.Property(p => p.DeletadoEm)
    .HasColumnName("deletado_em");

// Filtro global: queries automáticas ignoram inativos
builder.HasQueryFilter(p => p.Ativo);
```

**`UsuarioAdminConfiguration.cs`** — mesma estrutura.

### 2.2 Atualizar `IProdutoRepository`

Adicionar método para buscar incluindo inativos (para auditoria/admin):
```csharp
Task<Produto?> ObterPorIdIncluindoInativoAsync(Guid id);
Task<List<Produto>> ObterTodosIncluindoInativosAsync();
```

### 2.3 Atualizar `ProdutoRepository`

Implementar soft delete sobrescrevendo `DeletarAsync`:
```csharp
public override async Task<bool> DeletarAsync(Guid id)
{
    var produto = await _context.Produtos
        .IgnoreQueryFilters()  // busca mesmo se inativo
        .FirstOrDefaultAsync(p => p.Id == id);

    if (produto == null) return false;

    produto.Ativo = false;
    produto.DeletadoEm = DateTime.UtcNow;
    await _context.SaveChangesAsync();
    return true;
}
```

Implementar métodos adicionais com `IgnoreQueryFilters()` para consultas admin.

Mesma lógica para `UsuarioAdminRepository`.

### 2.4 Gerar Migration

```bash
dotnet ef migrations add AddSoftDelete --project Neostore.Persistence --startup-project Neostore.Api
```

Migration deve adicionar colunas `ativo` (boolean NOT NULL DEFAULT true) e `deletado_em` (datetime nullable) nas tabelas `produtos` e `usuarios_admin`.

---

## Passo 3 — Application (`Neostore.Application`)

### 3.1 Atualizar `DeletarProdutoCommandHandler`

Comportamento muda automaticamente: o repositório já faz soft delete. Nenhuma mudança necessária no handler, desde que o repositório sobrescreva `DeletarAsync`.

### 3.2 Atualizar `ObterProdutosPaginadoQueryHandler`

O `HasQueryFilter` no EF Core já filtra `Ativo = true` automaticamente em todas as queries LINQ. Nenhuma mudança necessária.

### 3.3 Atualizar `ObterProdutoPorIdQueryHandler`

O filtro global cobre. Produto deletado retorna `null` → controller retorna `404`. Nenhuma mudança necessária.

### 3.4 Verificar `ExistePorSkuAsync` no repositório

Garantir que a verificação de SKU único **ignora produtos inativos**:
```csharp
// O HasQueryFilter já resolve, mas verificar se ExistePorSkuAsync
// usa o DbSet filtrado ou IgnoreQueryFilters()
```

---

## Passo 4 — API (`Neostore.Api`)

### 4.1 Nenhuma mudança nos controllers

O comportamento do `DELETE` permanece igual do ponto de vista HTTP:
- `204 No Content` → soft delete realizado
- `404 Not Found` → produto não encontrado (ou já deletado)

---

## Passo 5 — Testes

### 5.1 Atualizar testes existentes

**`DeletarProdutoCommandHandlerTests`** — os testes continuam válidos pois testam o handler que chama `DeletarAsync`. Verificar que o mock está correto.

### 5.2 Adicionar testes para o repositório

Criar `Neostore.Tests/Application/Handlers/Produto/SoftDeleteIntegrationTests.cs` (ou testes de repositório separados):
- Produto deletado não aparece em `ObterPaginadoAsync`
- Produto deletado não aparece em `ObterPorIdAsync`
- `ExistePorSkuAsync` ignora produto deletado (SKU pode ser reutilizado)
- `DeletarAsync` seta `Ativo = false` e `DeletadoEm` com data UTC

---

## Sequência de execução

```
1. Passo 1 → Domínio (entidades + interface)
2. Passo 2.1 → Configurations (mapeamento EF)
3. Passo 2.2/2.3 → Repositórios (DeletarAsync + métodos adicionais)
4. Passo 2.4 → Migration
5. Passo 5.1/5.2 → Testes
6. Smoke test manual via Swagger
```

---

## Decisões de design

| Decisão | Escolha | Alternativa descartada |
| :------- | :------ | :--------------------- |
| Campo de controle | `Ativo: bool` + `DeletadoEm: DateTime?` | `Ativo: bool` |
| Filtro automático | `HasQueryFilter` no EF Core | Filtro manual em cada query — propenso a vazamentos |
| Reutilização de SKU | Permitida após soft delete | Proibir — conflito com histórico de pedidos |
| Acesso a registros inativos | `IgnoreQueryFilters()` em métodos específicos de auditoria | Rota admin separada — over-engineering |
