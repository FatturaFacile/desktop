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

    /// <summary>
    /// LoggerMessage delegate invoked when a request begins execution.
    /// </summary>
    private static readonly Action<ILogger, string, Exception?> RequestStartedAction = LoggerMessage.Define<string>(
                                                                                                                    logLevel: LogLevel
                                                                                                                       .Information,
                                                                                                                    eventId: new
                                                                                                                        EventId(
                                                                                                                                id: 1000,
                                                                                                                                name: nameof
                                                                                                                                    (RequestStarted)),
                                                                                                                    formatString: GenericResource
                                                                                                                       .Logging_RequestStarted);

    /// <summary>
    /// LoggerMessage delegate invoked when a request successfully completes.
    /// </summary>
    private static readonly Action<ILogger, string, long, Exception?> RequestCompletedAction =
        LoggerMessage.Define<string, long>(
                                           logLevel: LogLevel.Information,
                                           eventId: new EventId(id: 1001, name: nameof(RequestCompleted)),
                                           formatString: GenericResource.Logging_RequestCompleted);

    /// <summary>
    /// LoggerMessage delegate invoked when a request execution fails with an error.
    /// </summary>
    private static readonly Action<ILogger, string, long, Exception?> RequestFailedAction =
        LoggerMessage.Define<string, long>(
                                           logLevel: LogLevel.Error,
                                           eventId: new EventId(id: 1002, name: nameof(RequestFailed)),
                                           formatString: GenericResource.Logging_RequestFailed);

    /// <summary>
    /// Logs the start of a request execution.
    /// </summary>
    /// <param name="logger">The logger used to write the log entry.</param>
    /// <param name="requestName">The name of the request being executed.</param>
    public static void RequestStarted(
        ILogger logger,
        string  requestName) =>
        RequestStartedAction(arg1: logger, arg2: requestName, arg3: null);

    /// <summary>
    /// Logs the successful completion of a request execution.
    /// </summary>
    /// <param name="logger">The logger used to write the log entry.</param>
    /// <param name="requestName">The name of the request that was executed.</param>
    /// <param name="elapsedMilliseconds">The execution time, expressed in milliseconds.</param>
    public static void RequestCompleted(
        ILogger logger,
        string  requestName,
        long    elapsedMilliseconds) =>
        RequestCompletedAction(arg1: logger, arg2: requestName, arg3: elapsedMilliseconds, arg4: null);

    /// <summary>
    /// Logs a request that terminated with an unhandled exception.
    /// </summary>
    /// <param name="logger">The logger used to write the log entry.</param>
    /// <param name="exception">The exception that caused the request to fail.</param>
    /// <param name="requestName">The name of the request that failed.</param>
    /// <param name="elapsedMilliseconds">The execution time, expressed in milliseconds, before the failure occurred.</param>
    public static void RequestFailed(
        ILogger   logger,
        Exception exception,
        string    requestName,
        long      elapsedMilliseconds) =>
        RequestFailedAction(arg1: logger, arg2: requestName, arg3: elapsedMilliseconds, arg4: exception);

}
