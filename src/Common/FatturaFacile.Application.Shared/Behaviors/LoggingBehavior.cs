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
/// <param name="logger">
/// Logger used to record request execution details.
/// </param>
public sealed partial class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;

    /// <summary>
    /// Handles the incoming request and logs its execution details.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response from the next delegate.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the request or next delegate is null.</exception>
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
        {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        return HandleInternalAsync(request, next, cancellationToken);
    }

    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Processing command/query {RequestName}")]
    private static partial void RequestStarted(ILogger logger, string requestName);

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
    private static partial void RequestSucceeded(ILogger logger, string requestName);

    private static void ValidateParameters(
        TRequest request,
        RequestHandlerDelegate<TResponse> next)
        {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);
    }

    private Task<TResponse> HandleInternalAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
        {
        ValidateParameters(request, next);

        return HandleInternalCoreAsync(
            next,
            cancellationToken);
    }

    private async Task<TResponse> HandleInternalCoreAsync(
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
        {
        var requestName = typeof(TRequest).Name;

        RequestStarted(_logger, requestName);

        var response = await next(cancellationToken);

        LogResult(requestName, response);

        return response;
    }

    private void LogResult(string requestName, TResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (response is Result { IsFailure: true } failedResult)
        {
            RequestFailed(_logger, requestName, failedResult.ErrorCode, failedResult.Error);

            return;
        }

        RequestSucceeded(_logger, requestName);
    }
}
