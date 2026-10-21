// <copyright file="TransactionBehavior.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FatturaFacile.Application.Common.Behaviors;

/// <summary>
/// Represents transaction behavior.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="logger">The logger.</param>
/// <typeparam name="TRequest">The type of the t request.</typeparam>
/// <typeparam name="TResponse">The type of the t response.</typeparam>
public sealed class TransactionBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork unitOfWork = unitOfWork;

    /// <summary>
    /// Handle.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="next">The next.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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

        _ = await unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}
