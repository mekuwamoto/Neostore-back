# Neostore.Application

Use-case layer. Orchestrates domain + persistence via CQRS (MediatR). No HTTP, no EF Core.

## Structure

```
Behaviors/      — MediatR pipeline behaviors
Commands/       — Write intentions (immutable records)
  Categoria/
  Produto/
  UsuarioAdmin/
DTOs/           — Read-only response shapes
Handlers/       — Command and query processors
  Categoria/
  Produto/
Mappings/       — AutoMapper profiles
Queries/        — Read intentions (immutable records)
  Categoria/
  Produto/
  UsuarioAdmin/
Validators/     — FluentValidation (one per command)
DependencyInjection.cs
```

## Rules

- Commands and queries are `record` types implementing `IRequest<TResponse>`.
- Handlers implement `IRequestHandler<TRequest, TResponse>`.
- Business rule violations throw `InvalidOperationException` — caught by `ExceptionMiddleware` in Api → 400.
- Never reference `Microsoft.AspNetCore.*` or EF Core here.
- Never use `var` — always explicit types.

## Commands and Queries

### Categoria

| Type | Class | Returns |
|------|-------|---------|
| Command | `CriarCategoriaCommand(Nome, IdCategoriaPai?)` | `CategoriaDto` |
| Command | `AtualizarCategoriaCommand(Id, Nome, IdCategoriaPai?)` | `CategoriaDto` |
| Command | `DeletarCategoriaCommand(Id)` | `bool` |
| Query | `ObterTodasCategoriasQuery` | `List<CategoriaDto>` |
| Query | `ObterCategoriaPorIdQuery(Id)` | `CategoriaDto?` |

### Produto

| Type | Class | Returns |
|------|-------|---------|
| Command | `CriarProdutoCommand(Nome, SKU, Preco, IdCategoria, Descricao)` | `ProdutoDto` |
| Command | `AtualizarProdutoCommand(Id, Nome, SKU, Preco, IdCategoria, Descricao)` | `ProdutoDto` |
| Command | `DeletarProdutoCommand(Id)` | `bool` |
| Command | `AjustarEstoqueCommand(IdProduto, NovaQuantidade)` | `int` |
| Query | `ObterTodosProdutosQuery` | `List<ProdutoDto>` |
| Query | `ObterProdutoPorIdQuery(Id)` | `ProdutoDto?` |
| Query | `ObterProdutosPaginadoQuery(...)` | `ProdutosPaginadoDto` |

### UsuarioAdmin (commands/queries defined, handlers not yet implemented)

| Type | Class | Returns |
|------|-------|---------|
| Command | `CriarUsuarioAdminCommand` | — |
| Command | `AtualizarSenhaCommand` | — |
| Command | `DeletarUsuarioAdminCommand` | — |
| Query | `ObterTodosUsuariosAdminQuery` | — |
| Query | `ObterUsuarioAdminPorIdQuery` | — |

## DTOs

| DTO | Fields |
|-----|--------|
| `CategoriaDto` | Id, Nome, Slug, IdCategoriaPai? |
| `ProdutoDto` | Id, Nome, SKU, Preco, IdCategoria, Descricao, `List<ImagemDto>` Imagens, Estoque |
| `ImagemDto` | Id, NomeArquivo, ChaveS3, TipoConteudo, TamanhoBytes, DataCriacao |
| `UsuarioAdminDto` | Id, Email, Role (no SenhaHash) |

## AutoMapper

`MappingProfile` in `Mappings/MappingProfile.cs` maps:
- `Categoria → CategoriaDto`
- `Produto → ProdutoDto`
- `Imagem → ImagemDto`

All field names are identical — no `.ForMember()` required. Always use two-type-param overload:
```csharp
mapper.Map<Produto, ProdutoDto>(produto)
```

## Pipeline Behaviors

`LoggingBehavior<TRequest, TResponse>` in `Behaviors/LoggingBehavior.cs`:
- Logs operation name + request payload at `Information` before execution.
- Logs completion + elapsed ms at `Information` after execution.
- Applied to all requests — registered as open generic in DI.

Execution order: **Validator → LoggingBehavior → Handler**

## Validators

One validator per command. FluentValidation auto-wired into MediatR pipeline — invalid commands never reach the handler.

| Validator | Key Rules |
|-----------|-----------|
| `CriarCategoriaCommandValidator` | Nome required, min 2 chars |
| `AtualizarCategoriaCommandValidator` | Id required + Nome rules |
| `CriarProdutoCommandValidator` | Nome, SKU, Preco (>0), IdCategoria required |
| `AtualizarProdutoCommandValidator` | Id required + Produto rules |
| `CriarUsuarioAdminCommandValidator` | Email format, Senha, Role required |
| `AtualizarSenhaCommandValidator` | Old/new password rules |

## DependencyInjection.cs

`AddApplication()` registers:
```csharp
services.AddMediatR(typeof(DependencyInjection));
services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddAutoMapper(typeof(DependencyInjection).Assembly);
```

## Adding a New Command/Query

1. Create `record` in `Commands/` or `Queries/` implementing `IRequest<TResponse>`.
2. Create handler in `Handlers/` implementing `IRequestHandler<TRequest, TResponse>`.
3. Create validator in `Validators/` if it's a command.
4. Add handler test in `Neostore.Tests/Application/Handlers/`.
5. No DI registration needed — MediatR and FluentValidation scan by convention.
