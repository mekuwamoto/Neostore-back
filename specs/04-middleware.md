# ADR-04: Logging Estruturado e Middleware de Exceções

## Status
Proposed

## Date
2025-01-01

## Context
ADR-02 define requisito transversal pendente: registrar usuário executor, operação, timestamp e id do registro em cada operação CRUD. Atualmente, tratamento de erros está disperso em `try/catch` nos controllers — duplicação e comportamento inconsistente.

## Decision
Adotar **Serilog** para logging estruturado + **ExceptionMiddleware** centralizado + **MediatR Pipeline Behavior** para auditoria de commands/queries.

### Justificativa: Serilog

| Critério | Serilog |
| -------- | ------- |
| Logging estruturado nativo | ✅ Propriedades tipadas, indexáveis em qualquer sink |
| Integração ASP.NET Core | `UseSerilog()` substitui `ILogger<T>` padrão — zero mudança nos handlers |
| Configuração | Via `appsettings.json` — troca de sink sem alterar código |
| Custo | Gratuito (MIT) |

### Pacotes

```bash
# Neostore.Api
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

### Configuração Serilog (`Startup.cs`)

```csharp
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));
```

**`appsettings.json`:**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": { "Microsoft": "Warning", "System": "Warning" }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/neostore-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName"]
  }
}
```

### ExceptionMiddleware (`Neostore.Api/Middlewares/ExceptionMiddleware.cs`)

```csharp
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de negócio: {Mensagem}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno não tratado");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { erro = "Erro interno do servidor." });
        }
    }
}
```

Registro em `MiddlewareConfiguration.cs`:
```csharp
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
```

### LoggingBehavior (`Neostore.Application/Behaviors/LoggingBehavior.cs`)

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string operacao = typeof(TRequest).Name;

        _logger.LogInformation("Iniciando operação {Operacao} | Dados: {@Request}", operacao, request);

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        TResponse response = await next();
        stopwatch.Stop();

        _logger.LogInformation("Operação {Operacao} concluída em {ElapsedMs}ms", operacao, stopwatch.ElapsedMilliseconds);

        return response;
    }
}
```

Registro em `Neostore.Application/DependencyInjection.cs`:
```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

### Impacto nos Controllers
Com `ExceptionMiddleware` ativo, remover todos os `try/catch` dos controllers:
```csharp
[HttpPost]
public async Task<ActionResult<ProdutoDto>> Criar([FromBody] CriarProdutoCommand command)
{
    ProdutoDto resultado = await _mediator.Send(command);
    return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
}
```

## Consequences
### Positivo
- Tratamento de erros centralizado — controllers mais simples.
- Logging estruturado permite indexação e alertas em qualquer sink (Seq, CloudWatch, etc.).
- `LoggingBehavior` registra automaticamente toda operação MediatR sem modificar handlers.

### Trade-offs
- Serilog adiciona dependência externa (mitigado: MIT, amplamente adotado).
- `LoggingBehavior` loga payload completo do request — dados sensíveis devem ser anonimizados futuramente.

## Responsabilidades por Camada

| Evento | Nível | Responsável |
| ------ | ----- | ----------- |
| Requisição HTTP recebida | `Information` | Serilog built-in (`UseSerilogRequestLogging`) |
| Início de Command/Query | `Information` | `LoggingBehavior` |
| Conclusão de Command/Query | `Information` | `LoggingBehavior` |
| Erro de negócio (`InvalidOperationException`) | `Warning` | `ExceptionMiddleware` |
| Erro de sistema | `Error` | `ExceptionMiddleware` |

## Sequência de Implementação

1. Instalar pacotes Serilog
2. Configurar `UseSerilog()` em `Startup.cs` + seção no `appsettings.json`
3. Criar `ExceptionMiddleware` + registrar em `MiddlewareConfiguration`
4. Criar `LoggingBehavior` + registrar em `DependencyInjection`
5. Remover `try/catch` dos controllers
6. Verificar: `InvalidOperationException` → 400; exceção genérica → 500
