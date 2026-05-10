# ADR-03: Soft Delete em Produto e UsuarioAdmin

## Status
Proposed

## Date
2025-01-01

## Context
`DeletarProdutoCommand` executa hard-delete. Pedidos futuros referenciam produtos — excluir fisicamente quebraria histórico. Mesmo raciocínio para `UsuarioAdmin` e trilha de auditoria.

`Categoria` usa hard-delete e permanece assim: a exclusão já é bloqueada por regras de negócio quando existem produtos ou subcategorias vinculadas.

## Decision
Implementar **soft-delete** via interface `ISoftDeletable` + `HasQueryFilter` no EF Core.

| Entidade | Soft Delete | Justificativa |
| -------- | ----------- | ------------- |
| `Produto` | ✅ Sim | Preservar histórico de pedidos |
| `UsuarioAdmin` | ✅ Sim | Preservar trilha de auditoria |
| `Categoria` | ❌ Não | Exclusão bloqueada por regras de negócio |

### Campos adicionados

```csharp
// Neostore.Domain/Interfaces/ISoftDeletable.cs
public interface ISoftDeletable
{
    bool Ativo { get; set; }
    DateTime? DeletadoEm { get; set; }
}
```

`Produto` e `UsuarioAdmin` implementam `ISoftDeletable`:
```csharp
public bool Ativo { get; set; } = true;
public DateTime? DeletadoEm { get; set; }
```

### Persistência

**EF Core Configurations** (`ProdutoConfiguration`, `UsuarioAdminConfiguration`):
```csharp
builder.Property(p => p.Ativo).HasColumnName("ativo").IsRequired().HasDefaultValue(true);
builder.Property(p => p.DeletadoEm).HasColumnName("deletado_em");
builder.HasQueryFilter(p => p.Ativo);  // filtra automaticamente em todas as queries
```

**`ProdutoRepository.DeletarAsync`** — sobrescreve base:
```csharp
public override async Task<bool> DeletarAsync(Guid id)
{
    Produto? produto = await _context.Produtos
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(p => p.Id == id);

    if (produto == null) return false;

    produto.Ativo = false;
    produto.DeletadoEm = DateTime.UtcNow;
    await _context.SaveChangesAsync();
    return true;
}
```

Mesma lógica para `UsuarioAdminRepository`.

**Métodos adicionais** para auditoria admin (com `IgnoreQueryFilters()`):
```csharp
Task<Produto?> ObterPorIdIncluindoInativoAsync(Guid id);
Task<List<Produto>> ObterTodosIncluindoInativosAsync();
```

### Application e API
Nenhuma mudança necessária em handlers ou controllers:
- `HasQueryFilter` filtra automaticamente — handlers de leitura retornam `null` para inativos → controller retorna `404`.
- `DELETE` mantém contrato HTTP: `204 No Content` (soft-delete realizado) ou `404 Not Found`.

### Migration
```bash
dotnet ef migrations add AddSoftDelete --project Neostore.Persistence --startup-project Neostore.Api
```
Adiciona colunas `ativo` (boolean NOT NULL DEFAULT true) e `deletado_em` (datetime nullable) em `produtos` e `usuarios_admin`.

## Consequences
### Positivo
- Histórico de produtos preservado para pedidos futuros.
- Filtro global no EF Core elimina vazamentos em queries LINQ.
- SKU pode ser reutilizado após soft-delete.
- Handlers e controllers não precisam de alteração.

### Trade-offs
- Necessário `IgnoreQueryFilters()` explícito em métodos de auditoria para acessar registros inativos.
- Migration adicional necessária.
- Banco cresce ao longo do tempo (registros nunca removidos fisicamente).

## Decisões de Design

| Decisão | Escolha | Alternativa descartada |
| ------- | ------- | ---------------------- |
| Campo de controle | `Ativo: bool` + `DeletadoEm: DateTime?` | Somente `Ativo: bool` |
| Filtro automático | `HasQueryFilter` no EF Core | Filtro manual em cada query — propenso a vazamentos |
| Reutilização de SKU | Permitida após soft-delete | Proibir — conflito com histórico de pedidos |
| Acesso a inativos | `IgnoreQueryFilters()` em métodos específicos | Rota admin separada — over-engineering |

## Sequência de Implementação

1. Domínio: `ISoftDeletable` + campos em `Produto` e `UsuarioAdmin`
2. Persistência: Configurations (mapeamento EF) + `DeletarAsync` + métodos adicionais
3. Migration: `dotnet ef migrations add AddSoftDelete`
4. Testes: atualizar `DeletarProdutoCommandHandlerTests` + testes de repositório

## Testes Necessários
- Produto deletado não aparece em `ObterPaginadoAsync`
- Produto deletado não aparece em `ObterPorIdAsync`
- `ExistePorSkuAsync` ignora produto deletado (SKU reutilizável)
- `DeletarAsync` seta `Ativo = false` e `DeletadoEm` com timestamp UTC
