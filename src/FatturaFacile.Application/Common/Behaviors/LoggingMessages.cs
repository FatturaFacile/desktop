// <copyright file="LoggingMessages.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Resources;
using Microsoft.Extensions.Logging;

namespace FatturaFacile.Application.Common.Behaviors;

/// <summary>
/// Provides strongly typed logging messages for application request processing.
/// </summary>
internal static class LoggingMessages
{
    private static readonly Action<ILogger, string, Exception?> RequestStartedAction =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1000, nameof(RequestStarted)),
            GenericResource.Logging_RequestStarted);

    private static readonly Action<ILogger, string, long, Exception?> RequestCompletedAction =
        LoggerMessage.Define<string, long>(
            LogLevel.Information,
            new EventId(1001, nameof(RequestCompleted)),
            GenericResource.Logging_RequestCompleted);

    private static readonly Action<ILogger, string, long, Exception?> RequestFailedAction =
        LoggerMessage.Define<string, long>(
            LogLevel.Error,
            new EventId(1002, nameof(RequestFailed)),
            GenericResource.Logging_RequestFailed);

    /// <summary>
    /// Logs the start of a request execution.
    /// </summary>
    /// <param name="logger">The logger used to write the log entry.</param>
    /// <param name="requestName">The name of the request being executed.</param>
    public static void RequestStarted(
        ILogger logger,
        string requestName) =>
        RequestStartedAction(logger, requestName, null);

    /// <summary>
    /// Logs the successful completion of a request execution.
    /// </summary>
    /// <param name="logger">The logger used to write the log entry.</param>
    /// <param name="requestName">The name of the request that was executed.</param>
    /// <param name="elapsedMilliseconds">The execution time, expressed in milliseconds.</param>
    public static void RequestCompleted(
        ILogger logger,
        string requestName,
        long elapsedMilliseconds) =>
        RequestCompletedAction(logger, requestName, elapsedMilliseconds, null);

    /// <summary>
    /// Logs a request that terminated with an unhandled exception.
    /// </summary>
    /// <param name="logger">The logger used to write the log entry.</param>
    /// <param name="exception">The exception that caused the request to fail.</param>
    /// <param name="requestName">The name of the request that failed.</param>
    /// <param name="elapsedMilliseconds">The execution time, expressed in milliseconds, before the failure occurred.</param>
    public static void RequestFailed(
        ILogger logger,
        Exception exception,
        string requestName,
        long elapsedMilliseconds) =>
        RequestFailedAction(logger, requestName, elapsedMilliseconds, exception);
}
