// <copyright file="LoggingBehavior.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FatturaFacile.Application.Shared.Behaviors;

/// <summary>
/// Pipeline behavior that logs execution metrics, incoming request details, and outcome results for MediatR requests.
/// </summary>
/// <typeparam name="TRequest">The type of incoming request.</typeparam>
/// <typeparam name="TResponse">The type of returned response.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="LoggingBehavior{TRequest, TResponse}"/> class.
/// </remarks>
/// <param name="logger">The structured logger instance.</param>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Processing command/query {RequestName}", requestName);

        var response = await next(cancellationToken);

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
