# ADR-09: Integração Front-Back — CORS

**Status:** Implemented

## Contexto

Angular SPA em `http://localhost:4200` chama API em `http://localhost:5085`. Browser envia preflight `OPTIONS` antes de qualquer `POST`/`PUT`/`DELETE`. API bloqueava com dois problemas distintos:

1. `UseHttpsRedirection()` antes de CORS — preflight `OPTIONS` recebia `307` antes dos headers `Access-Control-Allow-*`.
2. Após mover CORS para antes do redirect, o `POST` real continuava recebendo `307` — `UseHttpsRedirection` redirecionava requisições HTTP em desenvolvimento.

## Causa Raiz

Dois problemas em sequência:

| Problema | Sintoma | Causa |
|---|---|---|
| #1 | `OPTIONS` → `307` | `UseHttpsRedirection` antes de `UseCors` no pipeline |
| #2 | `OPTIONS` → `204`, `POST` → `307` | `UseHttpsRedirection` redireciona requisições HTTP mesmo em dev |

## Decisão

1. Criar `CorsOptions` (Options Pattern) — origins lidas de `appsettings.json`, sem hardcode.
2. Registrar política CORS `"AllowFrontend"` via `IConfiguration`.
3. `UseCors` **antes** de `UseHttpsRedirection` no pipeline.
4. `UseHttpsRedirection` condicional — ativo apenas fora de Development.

Pipeline final:

```
ExceptionMiddleware → UseCors → UseHttpsRedirection (não-dev) → UseAuthorization
```

## Implementação

### 1. `Neostore.Api/Options/CorsOptions.cs`

```csharp
namespace Neostore.Api.Options;

public sealed class CorsOptions
{
    public const string SectionName = "Cors";
    public string[] AllowedOrigins { get; init; } = [];
}
```

### 2. `appsettings.json`

```json
"Cors": {
  "AllowedOrigins": []
}
```

### 3. `appsettings.Development.json`

```json
"Cors": {
  "AllowedOrigins": ["http://localhost:4200"]
}
```

### 4. `Neostore.Api/Services/DependencyInjection.cs`

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

### 5. `Neostore.Api/Middlewares/MiddlewareConfiguration.cs`

```csharp
internal static WebApplication ConfigureMiddlewares(this WebApplication app)
{
    app.UseMiddleware<ExceptionMiddleware>();
    app.UseCors("AllowFrontend");
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    app.UseAuthorization();
    return app;
}
```

## Consequências

- Preflight `OPTIONS` responde `204` com headers CORS corretos.
- Requisições HTTP em Development não são redirecionadas — Angular `http://localhost:4200` funciona sem TLS local.
- Em produção, `UseHttpsRedirection` ativo + origins configuradas via `appsettings.json` do ambiente.
- Sem impacto nos testes unitários existentes.
