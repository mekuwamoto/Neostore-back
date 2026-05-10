# Neostore.Persistence

EF Core persistence layer. Implements repository interfaces from Domain. No business logic here.

## Structure

```
Context/
  NeostoreDbContext.cs
  Configurations/
    CategoriaConfiguration.cs
    ProdutoConfiguration.cs
    ImagemConfiguration.cs
    UsuarioAdminConfiguration.cs
Repositories/
  IRepository.cs
  ICategoriaRepository.cs
  IProdutoRepository.cs
  IUsuarioAdminRepository.cs
  Repository.cs
  CategoriaRepository.cs
  ProdutoRepository.cs
  UsuarioAdminRepository.cs
DependencyInjection.cs
```

## Rules

- No business logic — delegate invariants to domain entities.
- Never use `var` — always explicit types.
- Always call `SaveChangesAsync()` — never `SaveChanges()`.
- Configurations live in their own file per entity — never configure in `OnModelCreating` directly.

## DbContext

`NeostoreDbContext` — 4 DbSets:
- `DbSet<Categoria> Categorias`
- `DbSet<Produto> Produtos`
- `DbSet<Imagem> Imagens`
- `DbSet<UsuarioAdmin> UsuariosAdmin`

Configurations applied via `ApplyConfigurationsFromAssembly()`.

## Entity Configurations

| File | Table | Key Config |
|------|-------|------------|
| `CategoriaConfiguration` | `categorias` | Unique index on Nome+Slug; FK self-ref cascade = Restrict |
| `ProdutoConfiguration` | `produtos` | Unique index on SKU; FK to Categoria (Restrict); cascade delete to Imagens; `HasQueryFilter(p => p.Ativo)` |
| `ImagemConfiguration` | `imagens` | FK to Produto (Cascade); index on IdProduto |
| `UsuarioAdminConfiguration` | `usuarios_admin` | Unique index on Email; `HasQueryFilter(u => u.Ativo)` |

**Important:** `Produto` and `UsuarioAdmin` have global query filters — inactive records are excluded automatically from all LINQ queries. Use `IgnoreQueryFilters()` to access them in audit/admin methods.

## Repository Pattern

### Generic Base (`IRepository<T>` / `Repository<T>`)
```
ObterPorIdAsync(Guid)
ObterTodosAsync()
CriarAsync(T)
AtualizarAsync(T)
DeletarAsync(Guid)
SaveChangesAsync()
```

### ICategoriaRepository
Extends base +
- `ObterPorSlugAsync(slug)` — lookup by URL slug
- `ObterArvoreAsync()` — hierarchical tree structure
- `ObterRaizAsync()` — categories without parent (`IdCategoriaPai == null`)
- `ExistePorNomeAsync(nome)` — uniqueness check
- `ContarProdutosAsync(id)` — blocks deletion if > 0
- `ContarSubcategoriasAsync(id)` — blocks deletion if > 0

### IProdutoRepository
Extends base +
- `ObterPorSkuAsync(sku)`
- `ObterPaginadoAsync(skip, take, filtros...)` — paginated with optional filters (IdCategoria, Nome partial, SKU)
- `ContarTotalAsync(filtros...)` — total count for pagination
- `ExistePorSkuAsync(sku, excludeId?)` — SKU uniqueness, excluding self on update
- `ObterComImagensAsync(id)` — eager loads `Imagens` collection
- `ObterPorIdIncluindoInativoAsync(id)` — bypasses `HasQueryFilter` for audit

### IUsuarioAdminRepository
Extends base +
- `ObterPorEmailAsync(email)`
- `ObterPorIdIncluindoInativoAsync(id)` — bypasses `HasQueryFilter`

## Soft Delete

`ProdutoRepository.DeletarAsync` and `UsuarioAdminRepository.DeletarAsync` override the base:
```csharp
produto.Ativo = false;
produto.DeletadoEm = DateTime.UtcNow;
await _context.SaveChangesAsync();
```
Hard deletes are **not** used for these entities. `CategoriaRepository` uses hard delete (blocked by business rules at handler level).

## DependencyInjection.cs

`AddPersistence(IServiceCollection, IConfiguration)` registers:
- `NeostoreDbContext` with MySql (Pomelo) connection string from config
- `ICategoriaRepository → CategoriaRepository` (Scoped)
- `IProdutoRepository → ProdutoRepository` (Scoped)
- `IUsuarioAdminRepository → UsuarioAdminRepository` (Scoped)

## Migrations

Not yet generated. To create initial migration:
```bash
# From repo root
dotnet ef migrations add InitialCreate --project src/Neostore.Persistence --startup-project src/Neostore.Api
```

## Adding a New Repository

1. Define interface in `Repositories/INovaEntidadeRepository.cs` extending `IRepository<NovaEntidade>`.
2. Implement in `Repositories/NovaEntidadeRepository.cs` extending `Repository<NovaEntidade>`.
3. Create `Context/Configurations/NovaEntidadeConfiguration.cs` implementing `IEntityTypeConfiguration<NovaEntidade>`.
4. Add `DbSet<NovaEntidade>` to `NeostoreDbContext`.
5. Register in `DependencyInjection.cs`.
