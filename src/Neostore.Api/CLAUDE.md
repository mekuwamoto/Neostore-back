# Neostore.Api

Presentation layer. HTTP in, MediatR out. No business logic here.

## Structure

```
Controllers/
  CategoriaController.cs
  ProdutoController.cs
Middlewares/
  ExceptionMiddleware.cs
  MiddlewareConfiguration.cs
Services/
  DependencyInjection.cs
Startup.cs
appsettings.json
appsettings.Development.json
```

## Rules

- Controllers only dispatch to MediatR — no business logic, no `try/catch`.
- All error handling via `ExceptionMiddleware` — do not add `try/catch` to controllers.
- Never inject repositories directly into controllers — always go through MediatR.
- Register services in `Services/DependencyInjection.cs`, middleware in `Middlewares/MiddlewareConfiguration.cs`.
- Never use `var` — always explicit types.

## Controllers

### CategoriaController (`/api/admin/categorias`)

| Method | Route | Command/Query | Response |
|--------|-------|---------------|----------|
| `POST` | `/` | `CriarCategoriaCommand` | `201 CategoriaDto` |
| `GET` | `/` | `ObterTodasCategoriasQuery` | `200 List<CategoriaDto>` |
| `GET` | `/{id}` | `ObterCategoriaPorIdQuery` | `200 CategoriaDto` / `404` |
| `PUT` | `/{id}` | `AtualizarCategoriaCommand` | `200 CategoriaDto` / `404` |
| `DELETE` | `/{id}` | `DeletarCategoriaCommand` | `204` / `400` / `404` |

### ProdutoController (`/api/admin/produtos`)

| Method | Route | Command/Query | Response |
|--------|-------|---------------|----------|
| `POST` | `/` | `CriarProdutoCommand` | `201 ProdutoDto` |
| `GET` | `/` | `ObterProdutosPaginadoQuery` | `200 ProdutosPaginadoDto` |
| `GET` | `/{id}` | `ObterProdutoPorIdQuery` | `200 ProdutoDto` / `404` |
| `PUT` | `/{id}` | `AtualizarProdutoCommand` | `200 ProdutoDto` / `404` |
| `PATCH` | `/{id}/estoque` | `AjustarEstoqueCommand` | `200 int` / `400` / `404` |
| `DELETE` | `/{id}` | `DeletarProdutoCommand` | `204` / `404` |

Controller pattern:
```csharp
[HttpPost]
[ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<CategoriaDto>> Criar([FromBody] CriarCategoriaCommand command)
{
    CategoriaDto resultado = await _mediator.Send(command);
    return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
}
```

## Middleware

### ExceptionMiddleware (`Middlewares/ExceptionMiddleware.cs`)
Centralizes error handling for the entire pipeline:
- `InvalidOperationException` → `400 Bad Request` + `{ erro: message }`
- Any other `Exception` → `500 Internal Server Error` + `{ erro: "Erro interno do servidor." }`
- Logs `Warning` for business errors, `Error` for system errors via `ILogger`.

### MiddlewareConfiguration (`Middlewares/MiddlewareConfiguration.cs`)
`ConfigureMiddlewares(WebApplication)` registration order:
1. `UseMiddleware<ExceptionMiddleware>()`
2. `UseHttpsRedirection()`
3. `UseAuthorization()`

Order matters — `ExceptionMiddleware` must be first.

## DI Composition (`Services/DependencyInjection.cs`)

`AddServices(IServiceCollection, IConfiguration)` calls:
```csharp
services.AddApplication();
services.AddInfrastructure();
services.AddPersistence(configuration);
services.AddOpenApi(...);  // title, version, description
```

`AppApiServices(WebApplication)` calls:
```csharp
app.MapOpenApi();             // GET /openapi/v1.json
app.MapScalarApiReference();  // GET /scalar/v1
app.ConfigureMiddlewares();
app.MapControllers();
```

## Logging (Serilog)

Configured in `Startup.cs` via `builder.Host.UseSerilog()`. Config in `appsettings.json`:
- Sinks: Console + rolling File (`logs/neostore-.log`, 7-day retention)
- Min level: `Information`; Microsoft/System overridden to `Warning`
- Enrichers: `FromLogContext`, `WithMachineName`

## API Documentation

| URL | Content |
|-----|---------|
| `GET /openapi/v1.json` | OpenAPI 3.x spec |
| `GET /scalar/v1` | Scalar interactive UI |

All controller actions must have `[ProducesResponseType]` for every possible HTTP status code.

## Adding a New Controller

1. Create `Controllers/NovaEntidadeController.cs` inheriting `ControllerBase`.
2. Inject `IMediator` via constructor.
3. Annotate all actions with `[ProducesResponseType]`.
4. No `try/catch` — `ExceptionMiddleware` handles all exceptions.
5. No business logic — dispatch to MediatR only.
