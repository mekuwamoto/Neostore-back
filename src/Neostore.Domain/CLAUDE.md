# Neostore.Domain

Pure domain layer — no external dependencies, no NuGet packages.

## Rules

- No references to EF Core, MediatR, or any infrastructure concern.
- No `using` of Application or Persistence namespaces.
- All business invariants enforced inside entity methods — not in handlers.
- Constructor must always produce a valid entity (never an invalid state).

## Entities

### Categoria (`Entities/Categoria.cs`)
| Field | Type | Rule |
|-------|------|------|
| `Id` | Guid | Required, assigned by caller |
| `Nome` | string | Required |
| `Slug` | string | Generated via `GerarSlug()` |
| `IdCategoriaPai` | Guid? | Nullable — root categories have none |

**Methods:**
- `GerarSlug(nome)` — static; removes diacritics, lowercases, replaces spaces with hyphens
- `ValidarHierarquia(categoriaPai)` — throws `InvalidOperationException` if `id == categoriaPai.Id`

### Produto (`Entities/Produto.cs`)
Implements `ISoftDeletable`.

| Field | Type | Rule |
|-------|------|------|
| `Id` | Guid | Required |
| `Nome` | string | Required |
| `SKU` | string | Required, unique enforced at persistence |
| `Preco` | decimal | Must be > 0 |
| `IdCategoria` | Guid | Required FK |
| `Descricao` | string | Optional |
| `Imagens` | `List<Imagem>` | Initialized empty |
| `Estoque` | int | Must be ≥ 0 |
| `Ativo` | bool | Default `true` |
| `DeletadoEm` | DateTime? | Set on soft-delete |

**Methods:**
- `AjustarEstoque(delta)` — throws `InvalidOperationException` if result < 0
- `AdicionarImagem(imagem)` — adds to list, sets `imagem.IdProduto`
- `RemoverImagem(idImagem)` — removes by Id; no-op if not found

### Imagem (`Entities/Imagem.cs`)
| Field | Type | Rule |
|-------|------|------|
| `Id` | Guid | Required |
| `NomeArquivo` | string | Required |
| `ChaveS3` | string | Required — path in S3 bucket |
| `TipoConteudo` | string | MIME type |
| `TamanhoBytes` | long | File size |
| `IdProduto` | Guid | FK to Produto |
| `DataCriacao` | DateTime | UTC timestamp |

**Methods:**
- `ObterUrlS3(bucketUrl)` — returns `$"{bucketUrl}/{ChaveS3}"`

### UsuarioAdmin (`Entities/UsuarioAdmin.cs`)
Implements `ISoftDeletable`.

| Field | Type | Rule |
|-------|------|------|
| `Id` | Guid | Required |
| `Email` | string | Required, unique enforced at persistence |
| `SenhaHash` | string | Required — never store plain text |
| `Role` | string | Ex: Admin, Gerente |
| `Ativo` | bool | Default `true` |
| `DeletadoEm` | DateTime? | Set on soft-delete |

**Methods:**
- `AtualizarSenha(novoHash)` — throws `InvalidOperationException` if `novoHash` is null or empty

## Interfaces

### ISoftDeletable (`Interfaces/ISoftDeletable.cs`)
```csharp
public interface ISoftDeletable
{
    bool Ativo { get; set; }
    DateTime? DeletadoEm { get; set; }
}
```
Implemented by `Produto` and `UsuarioAdmin`. `Categoria` does NOT implement — its deletion is blocked by business rules in the handler.

## Adding a New Entity

1. Create `Entities/NovaEntidade.cs` — no base class required.
2. If soft-delete needed, implement `ISoftDeletable`.
3. Enforce all invariants in the constructor and methods via `InvalidOperationException`.
4. Add domain tests in `Neostore.Tests/Domain/`.
