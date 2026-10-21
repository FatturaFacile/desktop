// <copyright file="LoggingBehavior.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using MediatR;
using Microsoft.Extensions.Logging;

namespace FatturaFacile.Application.Common.Behaviors;

/// <summary>
/// Represents a MediatR pipeline behavior that logs the execution of requests,
/// including start time, completion time and unhandled exceptions.
/// </summary>
/// <typeparam name="TRequest">
/// The type of the request being processed.
/// </typeparam>
/// <typeparam name="TResponse">
/// The type of the response returned by the request handler.
/// </typeparam>
/// <param name="logger">
/// The logger used to write diagnostic and error messages.
/// </param>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Handles the request, logs execution information and measures
    /// the total processing time.
    /// </summary>
    /// <param name="request">
    /// The request being processed.
    /// </param>
    /// <param name="next">
    /// Delegate representing the next action in the pipeline.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result
    /// contains the response returned by the request handler.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="next"/> is <see langword="null"/>.
    /// </exception>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var requestName = typeof(TRequest).Name;

        LoggingMessages.RequestStarted(
            logger,
            requestName);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await next(cancellationToken);

            stopwatch.Stop();

            LoggingMessages.RequestCompleted(
                logger,
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            LoggingMessages.RequestFailed(
                logger,
                exception,
                requestName,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
