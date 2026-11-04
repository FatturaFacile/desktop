// <copyright file="LoggingBehavior.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FatturaFacile.Application.Shared.Behaviors;

/// <summary>
/// Pipeline behavior that logs execution metrics,
/// incoming request details and outcome results
/// for MediatR requests.
/// </summary>
/// <typeparam name="TRequest">
/// The type of incoming request.
/// </typeparam>
/// <typeparam name="TResponse">
/// The type of returned response.
/// </typeparam>
/// <param name="logger">The logger instance to use for logging request details and outcomes.</param>
public sealed partial class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger =
        logger;

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        RequestStarted(_logger, requestName);

        var response = await next(cancellationToken);

        if (response is Result { IsFailure: true } failedResult)
        {
            RequestFailed(
                _logger,
                requestName,
                failedResult.ErrorCode,
                failedResult.Error);
        }
        else
        {
            RequestSucceeded(
                _logger,
                requestName);
        }

        return response;
    }

    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Processing command/query {RequestName}")]
    private static partial void RequestStarted(
        ILogger logger,
        string requestName);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "Request {RequestName} failed with error {ErrorCode}: {ErrorMessage}")]
    private static partial void RequestFailed(
        ILogger logger,
        string requestName,
        string? errorCode,
        string? errorMessage);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "Successfully processed {RequestName}")]
    private static partial void RequestSucceeded(
        ILogger logger,
        string requestName);
}
