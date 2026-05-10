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
| `Neostore.Application` | Use cases / application services |
| `Neostore.Domain` | Core business entities and domain logic; no dependencies on other projects |
| `Neostore.Infrastructure` | External services, cross-cutting concerns |
| `Neostore.Persistence` | Data access (repositories, DB context) |
| `Neostore.Tests` | xUnit unit tests |

**Dependency direction:** `Api → Application → Domain`; `Infrastructure` and `Persistence` both depend on `Domain` only. `Application` depends on `Infrastructure` and `Persistence` for DI registration purposes.

### Dependency Injection

Each layer exposes an extension method in its own `DependencyInjection.cs`. `Startup.cs` composes them via `builder.Services.AddServices(builder.Configuration)`, which internally calls `AddApplication()`, `AddInfrastructure()`, and `AddPersistence()`.

When adding services to a layer, register them in that layer's `DependencyInjection.cs`, not in `Startup.cs`.

### Testing

Tests use **xUnit**, **Moq**, and **AwesomeAssertions**, mas utiliza os mesmos métodos que o FluentAssertions. Não utilize Assert do XUnit Coverage is collected via Coverlet. 

## Git & CI/CD

Branch strategy:

- `feature/*` → push triggers CI build + auto-creates PR to `develop`
- `develop` → merged `feature/*` PRs trigger CI build + auto-creates PR to `main`
- `main` → production/stable

Workflows are in `.github/workflows/`. Both pipelines run `restore → build (Release) → test` before creating the PR.


## Padrão de nomenclatura de variáveis
A nomenclatura de id estrangeiro será sempre Id + entidade.
Ex:
```
IdCategoria
IdProduto
IdUsuario
```
