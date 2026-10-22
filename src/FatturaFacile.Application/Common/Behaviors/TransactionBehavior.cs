// <copyright file="TransactionBehavior.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FatturaFacile.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that automatically commits transactional unit of work changes upon the successful completion of commands.
/// </summary>
/// <typeparam name="TRequest">The type of the request being processed in the pipeline.</typeparam>
/// <typeparam name="TResponse">The type of the response produced by the request handler.</typeparam>
/// <param name="unitOfWork">The unit of work used to coordinate atomic persistence of domain entity state changes.</param>
/// <param name="logger">The logger instance used for recording transactional lifecycle operations.</param>
public sealed partial class TransactionBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork unitOfWork = unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> logger = logger;

    /// <summary>
    /// Executes the pipeline handler and persists pending database changes when the request represents a mutating command.
    /// </summary>
    /// <param name="request">The incoming request being handled.</param>
    /// <param name="next">The asynchronous delegate representing the subsequent behavior or handler in the pipeline.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the pipeline execution and returning <typeparamref name="TResponse"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="next"/> is <see langword="null"/>.</exception>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var response = await next(cancellationToken);

        // If request is a command (modifying state), save changes
        var requestName = typeof(TRequest).Name;
        if (!requestName.EndsWith(
                "Command",
                StringComparison.Ordinal))
        {
            return response;
        }

        LogCommittingTransaction(logger, requestName);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Committing transaction for {RequestName}")]
    private static partial void LogCommittingTransaction(
        ILogger logger,
        string requestName);
}
