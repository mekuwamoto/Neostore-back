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

Commands and queries live in `Neostore.Application/Commands/` and `Neostore.Application/Queries/`. Each file contains both the immutable `record` implementing `IRequest<TResponse>` and its handler implementing `IRequestHandler<TRequest, TResponse>`.

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

## AutoMapper

`MappingProfile` in `Neostore.Application/Mappings/MappingProfile.cs` maps:
- `Categoria → CategoriaDto`
- `Produto → ProdutoDto`
- `Imagem → ImagemDto`

Registered via `services.AddAutoMapper(typeof(DependencyInjection).Assembly)` in `Neostore.Application/DependencyInjection.cs`.

Always use the two-type-param overload:
```csharp
mapper.Map<TSource, TDestination>(source)
```

In tests, instantiate a real mapper — do not mock with Moq:
```csharp
IMapper mapper = new MapperConfiguration(cfg =>
    cfg.AddProfile<MappingProfile>()).CreateMapper();
```

## Middleware and Logging

- **`ExceptionMiddleware`** (`Neostore.Api/Middlewares/`) — centralizes error handling: `InvalidOperationException` → 400, unhandled exceptions → 500. Controllers have no `try/catch`.
- **`LoggingBehavior<TRequest, TResponse>`** (`Neostore.Application/Behaviors/`) — MediatR pipeline behavior that logs every command/query start, completion, and elapsed time.
- **Serilog** configured in `Startup.cs` via `UseSerilog()`. Sinks: Console + rolling File (`logs/neostore-.log`, 7-day retention). Config lives in `appsettings.json` under `"Serilog"`.

## API Documentation

- **Spec:** `GET /openapi/v1.json` (Microsoft.AspNetCore.OpenApi)
- **UI:** `GET /scalar/v1` (Scalar.AspNetCore)
- Controllers annotated with `[ProducesResponseType]` for all response codes.

## NuGet Packages

| Project | Key Packages |
|---|---|
| `Neostore.Api` | Microsoft.AspNetCore.OpenApi 10.0.7, Scalar.AspNetCore 2.14.11, Serilog.AspNetCore 10.0.0, Serilog.Sinks.Console 6.1.1, Serilog.Sinks.File 7.0.0 |
| `Neostore.Application` | MediatR 11.1.0, MediatR.Extensions.Microsoft.DependencyInjection 11.1.0, FluentValidation 11.9.2, FluentValidation.DependencyInjectionExtensions 11.9.2, AutoMapper 16.1.1 |
| `Neostore.Domain` | *(none)* |
| `Neostore.Infrastructure` | Microsoft.Extensions.Configuration.Abstractions 10.0.7, Microsoft.Extensions.DependencyInjection.Abstractions 10.0.7 |
| `Neostore.Persistence` | EF Core 9.0.1, Pomelo.EntityFrameworkCore.MySql 9.0.0, Microsoft.Extensions.Configuration.Abstractions 10.0.7 |
| `Neostore.Tests` | xUnit 2.9.3, xunit.runner.visualstudio 3.1.4, Moq 4.20.72, AwesomeAssertions 9.4.0, coverlet.collector 6.0.4, Microsoft.NET.Test.Sdk 17.14.1 |

## Testing

Tests use **xUnit**, **Moq**, and **AwesomeAssertions**. AwesomeAssertions uses the same API as FluentAssertions. **Do not use `Assert` from xUnit.**

Prioritize business rules over implementation details.

Test structure:
- `Application/Handlers/Categoria/` — 5 handler test files
- `Application/Handlers/Produto/` — 6 handler test files
- `Application/Handlers/UsuarioAdmin/` — 6 handler test files
- `Application/Validators/` — CategoriaValidatorsTests, ProdutoValidatorsTests
- `Domain/` — CategoriaTests, ProdutoTests, ImagemTests, UsuarioAdminTests

Coverage collected via Coverlet.

## Specifications (ADRs)

Architecture decisions and implementation specs live in `./specs/` — all in ADR format.

| File | Decision |
|------|----------|
| `00-Introduçao.md` | Scope — API admin focused on product catalog |
| `01-Definição esquemas adicionais sistema.md` | CQRS, MediatR, domain entities |
| `02-Implementação cruds.md` | CRUD implementation status and pending items |
| `03-soft-delete.md` | Soft delete strategy for Produto and UsuarioAdmin |
| `04-middleware.md` | Serilog + ExceptionMiddleware + LoggingBehavior |
| `05-swagger.md` | OpenAPI with Scalar UI |
| `06-automapper.md` | AutoMapper for DTO mapping |
| `07-crud-usuario-admin.md` | CRUD de Usuários Admin — handlers, controller, BCrypt |

ADR statuses: `Implemented` | `Partially Implemented` | `Proposed` | `Accepted`

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

<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
<!-- SPECKIT END -->
