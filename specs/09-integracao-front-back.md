# ADR-09: Integração Front-Back — CORS

**Status:** Accepted

## Contexto

Angular SPA em `http://localhost:4200` chama API em `http://localhost:5085`. Browser envia preflight `OPTIONS` antes de qualquer `POST`/`PUT`/`DELETE`. API retorna `307 Redirect` (via `UseHttpsRedirection`) — browser bloqueia.

```
Access to XMLHttpRequest at 'http://localhost:5085/api/admin/categorias'
from origin 'http://localhost:4200' has been blocked by CORS policy:
Response to preflight request doesn't pass access control check:
Redirect is not allowed for a preflight request.
```

## Causa Raiz

`UseHttpsRedirection()` está antes de qualquer política CORS no pipeline. Preflight `OPTIONS` recebe `307` antes de receber headers `Access-Control-Allow-*` — o browser interpreta como falha de CORS.

## Decisão

1. Registrar política CORS nomeada `"AllowFrontend"` em `DependencyInjection.cs`.
2. Adicionar `UseCors("AllowFrontend")` em `MiddlewareConfiguration.cs` **antes** de `UseHttpsRedirection()`.

Ordem correta do pipeline:

```
ExceptionMiddleware → UseCors → UseHttpsRedirection → UseAuthorization
```

## Implementação

### 1. `Neostore.Api/Options/CorsOptions.cs` (novo arquivo)

```csharp
namespace Neostore.Api.Options;

public sealed class CorsOptions
{
    public const string SectionName = "Cors";
    public string[] AllowedOrigins { get; init; } = [];
}
```

### 2. `appsettings.json` — adicionar seção

```json
"Cors": {
  "AllowedOrigins": []
}
```

### 3. `appsettings.Development.json` — override para dev

```json
"Cors": {
  "AllowedOrigins": ["http://localhost:4200"]
}
```

### 4. `Neostore.Api/Services/DependencyInjection.cs`

Adicionar no método `AddServices`:

```csharp
services.Configure<CorsOptions>(configuration.GetSection(CorsOptions.SectionName));

CorsOptions corsOptions = configuration
    .GetSection(CorsOptions.SectionName)
    .Get<CorsOptions>() ?? new CorsOptions();

services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(corsOptions.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
```

### 2. `Neostore.Api/Middlewares/MiddlewareConfiguration.cs`

```csharp
internal static IApplicationBuilder ConfigureMiddlewares(this IApplicationBuilder app)
{
    app.UseMiddleware<ExceptionMiddleware>();
    app.UseCors("AllowFrontend");
    app.UseHttpsRedirection();
    app.UseAuthorization();
    return app;
}
```

## Consequências

- Preflight `OPTIONS` recebe `204` com headers CORS corretos — sem redirect.
- Política restrita a `localhost:4200` — em produção, substituir pela origin do deploy do front.
- Sem impacto nos testes unitários existentes.

