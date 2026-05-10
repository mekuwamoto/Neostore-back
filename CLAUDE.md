# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

All commands are run from the `src/` directory unless noted otherwise.

```bash
dotnet restore                            # Restore NuGet packages
dotnet build                              # Build solution
dotnet test                               # Run all tests
dotnet test --filter Name=<TestMethod>    # Run a single test by method name
dotnet test --filter ClassName=<Class>    # Run all tests in a class
dotnet run --project Neostore.Api         # Run the API (http://localhost:5085)
```

From the repo root, the Docker image can be built with:

```bash
docker build -t neostore-back .
```

## Architecture

This is an **ASP.NET Core Web API** (.NET 10) following **Clean Architecture** (Onion Architecture). The solution lives under `src/Neostore.slnx` and is split into five projects:

| Project | Role |
|---|---|
| `Neostore.Api` | Presentation layer — controllers, middleware, DI composition root |
| `Neostore.Application` | Use cases via CQRS (MediatR handlers, commands, queries, validators, DTOs) |
| `Neostore.Domain` | Core business entities and domain logic; no external dependencies |
| `Neostore.Infrastructure` | External services, cross-cutting concerns (currently minimal) |
| `Neostore.Persistence` | EF Core DbContext, repository implementations, entity configurations |
| `Neostore.Tests` | xUnit unit tests |

**Dependency direction:** `Api → Application → Domain`; `Infrastructure` and `Persistence` both depend on `Domain` only. `Application` depends on `Infrastructure` and `Persistence` for DI registration purposes.

### Dependency Injection

Each layer exposes an extension method in its own `DependencyInjection.cs`. `Startup.cs` composes them via `builder.Services.AddServices(builder.Configuration)`, which internally calls `AddApplication()`, `AddInfrastructure()`, and `AddPersistence()`.

When adding services to a layer, register them in that layer's `DependencyInjection.cs`, not in `Startup.cs`.

## Domain Entities

| Entity | Key Fields | Methods | Relations |
|---|---|---|---|
| `Categoria` | Id (Guid), Nome, Slug, IdCategoriaPai (Guid?) | `GerarSlug()`, `ValidarHierarquia()` | self-referencing parent |
| `Produto` | Id (Guid), Nome, SKU, Preco (decimal), Estoque (int), IdCategoria (Guid), Imagens (List) | `AjustarEstoque()`, `AdicionarImagem()`, `RemoverImagem()` | Categoria (FK), Imagem (collection) |
| `Imagem` | Id (Guid), NomeArquivo, ChaveS3, TipoConteudo, TamanhoBytes, IdProduto (Guid), DataCriacao | `ObterUrlS3()` | Produto (FK) |
| `UsuarioAdmin` | Id (Guid), Email, SenhaHash, Role | `AtualizarSenha()` | — |

All IDs are `Guid` assigned manually (`ValueGeneratedNever`).

## CQRS Pattern (MediatR)

Commands and queries live in `Neostore.Application/`. Each command/query is an immutable `record` implementing `IRequest<TResponse>`. Handlers implement `IRequestHandler<TRequest, TResponse>`.

### Current Commands

| Command | Handler | Returns |
|---|---|---|
| `CriarCategoriaCommand` | `CriarCategoriaCommandHandler` | `CategoriaDto` |
| `AtualizarCategoriaCommand` | `AtualizarCategoriaCommandHandler` | `CategoriaDto` |
| `DeletarCategoriaCommand` | `DeletarCategoriaCommandHandler` | `bool` |
| `CriarProdutoCommand` | `CriarProdutoCommandHandler` | `ProdutoDto` |
| `AtualizarProdutoCommand` | `AtualizarProdutoCommandHandler` | `ProdutoDto` |
| `DeletarProdutoCommand` | `DeletarProdutoCommandHandler` | `bool` |
| `AjustarEstoqueCommand` | `AjustarEstoqueCommandHandler` | `int` (new stock) |
| `CriarUsuarioAdminCommand` | — | — |
| `AtualizarSenhaCommand` | — | — |
| `DeletarUsuarioAdminCommand` | — | — |

### Current Queries

| Query | Handler | Returns |
|---|---|---|
| `ObterTodasCategoriasQuery` | `ObterTodasCategoriasQueryHandler` | `List<CategoriaDto>` |
| `ObterCategoriaPorIdQuery` | `ObterCategoriaPorIdQueryHandler` | `CategoriaDto?` |
| `ObterTodosProdutosQuery` | `ObterTodosProdutosQueryHandler` | `List<ProdutoDto>` |
| `ObterProdutoPorIdQuery` | `ObterProdutoPorIdQueryHandler` | `ProdutoDto?` |
| `ObterProdutosPaginadoQuery` | `ObterProdutosPaginadoQueryHandler` | `ProdutosPaginadoDto` |
| `ObterTodosUsuariosAdminQuery` | — | — |
| `ObterUsuarioAdminPorIdQuery` | — | — |

## Repository Pattern

`Neostore.Persistence/Repositories/` provides:

- `IRepository<T>` / `Repository<T>` — generic async CRUD (`ObterPorIdAsync`, `ObterTodosAsync`, `CriarAsync`, `AtualizarAsync`, `DeletarAsync`, `SaveChangesAsync`)
- `ICategoriaRepository` — extends base + `ObterPorSlugAsync`, `ObterArvoreAsync`, `ObterRaizAsync`, `ExistePorNomeAsync`, `ContarProdutosAsync`, `ContarSubcategoriasAsync`
- `IProdutoRepository` — extends base + `ObterPorSkuAsync`, `ObterPaginadoAsync`, `ContarTotalAsync`, `ExistePorSkuAsync`, `ObterComImagensAsync`

## Persistence

- **ORM:** EF Core 9.0.1 + Pomelo.EntityFrameworkCore.MySql 9.0.0
- **DbContext:** `NeostoreDbContext` — 4 DbSets (Categorias, Produtos, Imagens, UsuariosAdmin)
- **Configurations:** Fluent API in `Context/Configurations/` — applied via `ApplyConfigurationsFromAssembly()`
  - `CategoriaConfiguration`: unique index on Nome+Slug; FK cascade = Restrict
- **Migrations:** not yet generated (code-first, schema applied manually or via EnsureCreated)

## Validation

FluentValidation validators in `Neostore.Application/Validators/` — one per command. Integrated into MediatR pipeline; automatically validated before handler execution.

## NuGet Packages

| Project | Key Packages |
|---|---|
| `Neostore.Api` | Microsoft.AspNetCore.OpenApi 10.0.7 |
| `Neostore.Application` | MediatR 11.1.0, FluentValidation 11.9.2 |
| `Neostore.Domain` | *(none)* |
| `Neostore.Infrastructure` | Microsoft.Extensions.Configuration.Abstractions 10.0.7 |
| `Neostore.Persistence` | EF Core 9.0.1, Pomelo.EntityFrameworkCore.MySql 9.0.0 |
| `Neostore.Tests` | xUnit 2.9.3, Moq 4.20.72, AwesomeAssertions 9.4.0, coverlet.collector 6.0.4 |

## Testing

Tests use **xUnit**, **Moq**, and **AwesomeAssertions**. AwesomeAssertions uses the same API as FluentAssertions. **Do not use `Assert` from xUnit.**
Priorize regras de negócio e não tanto em implementação
Test structure:
- `Application/Handlers/Categoria/` — 5 handler test files
- `Application/Handlers/Produto/` — 6 handler test files
- `Application/Validators/` — CategoriaValidatorsTests, ProdutoValidatorsTests
- `Domain/` — CategoriaTests, ProdutoTests, ImagemTests, UsuarioAdminTests

Coverage collected via Coverlet.

## Git & CI/CD

Branch strategy:

- `feature/*` → push triggers CI build + auto-creates PR to `develop`
- `develop` → merged `feature/*` PRs trigger CI build + auto-creates PR to `main`
- `main` → production/stable

Workflows are in `.github/workflows/`. Both pipelines run `restore → build (Release) → test` before creating the PR.

## Padrão de nomenclatura de variáveis

FK naming: `Id` + nome da entidade.

```
IdCategoria
IdProduto
IdUsuario
```

## Tipagem de variáveis

Sempre utilize tipos explícitos. Nunca utilize `var`.
