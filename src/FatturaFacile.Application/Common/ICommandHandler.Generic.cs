// <copyright file="ICommandHandler.Generic.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common;

/// <summary>
/// Defines a contract for processing a specific command type that returns a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TCommand">The command type being handled.</typeparam>
/// <typeparam name="TResult">The resulting data type produced by executing the command.</typeparam>
public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    /// <summary>
    /// Executes the business logic associated with the specified command.
    /// </summary>
    /// <param name="command">The command instance to process.</param>
    /// <param name="cancellationToken">Cancellation token to observe during processing.</param>
    /// <returns>A task representing the asynchronous operation, returning <typeparamref name="TResult"/> upon completion.</returns>
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
