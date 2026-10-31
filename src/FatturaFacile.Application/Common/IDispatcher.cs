// <copyright file="IDispatcher.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common;

/// <summary>
/// Provides an application-level mediator contract for dispatching CQRS commands and queries to their respective handlers.
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// Dispatches a command that produces a result of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    /// <param name="command">The command instance to send.</param>
    /// <param name="cancellationToken">Cancellation token to observe during command execution.</param>
    /// <returns>A task representing the command execution, containing the resulting value.</returns>
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches a command that produces no return value.
    /// </summary>
    /// <param name="command">The command instance to send.</param>
    /// <param name="cancellationToken">Cancellation token to observe during command execution.</param>
    /// <returns>A task representing the asynchronous command execution.</returns>
    Task<Unit> SendAsync(ICommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches a read query and yields the requested data projection.
    /// </summary>
    /// <typeparam name="TResult">The expected query return type.</typeparam>
    /// <param name="query">The query parameters to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token to observe during query execution.</param>
    /// <returns>A task representing the asynchronous query execution, containing the projected data.</returns>
    Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
