using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Neostore.Application.Behaviors;

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

        _logger.LogInformation(
            "Iniciando operação {Operacao} | Dados: {@Request}",
            operacao, request);

        Stopwatch stopwatch = Stopwatch.StartNew();

        TResponse response = await next();

        stopwatch.Stop();

        _logger.LogInformation(
            "Operação {Operacao} concluída em {ElapsedMs}ms",
            operacao, stopwatch.ElapsedMilliseconds);

        return response;
    }
}
