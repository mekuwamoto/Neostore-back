# Plano de Implementação: Middleware de Logs

Este documento especifica o roteiro técnico para implementar logging estruturado e middleware de auditoria conforme o requisito transversal definido em `02-Implementação cruds.md`:

> **Logs:** Registrar: Usuário Executor, Operação, Data/Hora, Id do Registro afetado.
> **Tratamento de Erros:** Exceções de negócio → `400 Bad Request`. Erros de sistema → `500`.

---

### Serilog

| Aspecto | Detalhe |
| :------ | :------ |
| **Pacotes** | `Serilog.AspNetCore`, `Serilog.Sinks.Console`, `Serilog.Sinks.File` |
| **Estilo** | Logging estruturado (propriedades tipadas, não concatenação de string) |
| **Sinks** | Console, arquivo, Seq, Elasticsearch, Application Insights, CloudWatch |
| **Integração** | `UseSerilog()` substitui o logger padrão do ASP.NET Core |
| **Enriquecimento** | `Enrich.FromLogContext()` propaga propriedades entre camadas automaticamente |
| **Custo** | Gratuito (MIT) |

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```


### Decisão

**Serilog** é a escolha para este projeto pelos seguintes motivos:
- Logging **estruturado** nativo — propriedades ficam indexáveis em qualquer sink
- Configuração simples via `appsettings.json`
- Substitui o `ILogger<T>` do ASP.NET Core — zero mudança nos handlers existentes
- Troca de sink (console → arquivo → Seq) sem alterar código

---

## Passo 1 — Instalar pacotes

```bash
# Em src/Neostore.Api
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File

# Em src/Neostore.Application (para o Pipeline Behavior)
# Sem pacote extra — usa Microsoft.Extensions.Logging já transitivo
```

---

## Passo 2 — Configurar Serilog no `Startup.cs`

**`Neostore.Api/Startup.cs`:**

```csharp
using Serilog;

// Antes de builder.Build()
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));
```

**`appsettings.json`** — adicionar seção:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
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

---

## Passo 3 — Middleware global de exceções

Criar `Neostore.Api/Middlewares/ExceptionMiddleware.cs`:

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

Registrar em `MiddlewareConfiguration.cs`:

```csharp
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
```

Com isso, os `try/catch` nos controllers podem ser **removidos** — o middleware centraliza o tratamento.

---

## Passo 4 — MediatR Pipeline Behavior de auditoria

Criar `Neostore.Application/Behaviors/LoggingBehavior.cs`:

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
        var operacao = typeof(TRequest).Name;

        _logger.LogInformation(
            "Iniciando operação {Operacao} | Dados: {@Request}",
            operacao, request);

        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();

        _logger.LogInformation(
            "Operação {Operacao} concluída em {ElapsedMs}ms",
            operacao, stopwatch.ElapsedMilliseconds);

        return response;
    }
}
```

Registrar em `Neostore.Application/DependencyInjection.cs`:

```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

---

## Passo 5 — Simplificar controllers

Com `ExceptionMiddleware` ativo, os `try/catch` dos controllers tornam-se redundantes. Os controllers ficam:

```csharp
[HttpPost]
public async Task<ActionResult<ProdutoDto>> Criar([FromBody] CriarProdutoCommand command)
{
    var resultado = await _mediator.Send(command);
    return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
}
```

---

## Sequência de execução

```
1. Passo 1 → Instalar pacotes (Serilog)
2. Passo 2 → Configurar Serilog em Startup.cs + appsettings.json
3. Passo 3 → Criar ExceptionMiddleware + registrar em MiddlewareConfiguration
4. Passo 4 → Criar LoggingBehavior + registrar em DependencyInjection
5. Passo 5 → Remover try/catch dos controllers
6. Testes: verificar que InvalidOperationException → 400, Exception → 500
```

---

## O que cada camada registra

| Evento | Nível | Responsável |
| :----- | :---- | :---------- |
| Requisição HTTP recebida | `Information` | Serilog built-in (`UseSerilogRequestLogging`) |
| Início de Command/Query | `Information` | `LoggingBehavior` |
| Conclusão de Command/Query | `Information` | `LoggingBehavior` |
| Erro de negócio (`InvalidOperationException`) | `Warning` | `ExceptionMiddleware` |
| Erro de sistema (exceção não tratada) | `Error` | `ExceptionMiddleware` |
