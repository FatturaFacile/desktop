using MediatR;
using Microsoft.Extensions.Logging;
using FatturaFacile.Application.Shared.Results;

namespace FatturaFacile.Application.Shared.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Processing command/query {RequestName}", requestName);

        var response = await next();

        if (response is Result { IsFailure: true } failedResult)
        {
            _logger.LogWarning("Request {RequestName} failed with error {ErrorCode}: {ErrorMessage}", 
                requestName, failedResult.ErrorCode, failedResult.Error);
        }
        else
        {
            _logger.LogInformation("Successfully processed {RequestName}", requestName);
        }

        return response;
    }
}
