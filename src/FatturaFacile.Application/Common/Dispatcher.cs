// <copyright file="Dispatcher.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using MediatR;

namespace FatturaFacile.Application.Common;

/// <summary>
/// Implements the <see cref="IDispatcher"/> interface by mediating CQRS commands and queries via MediatR's <see cref="ISender"/>.
/// </summary>
public class Dispatcher : IDispatcher
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="Dispatcher"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender used to route requests to their handlers.</param>
    public Dispatcher(ISender sender)
    {
        _sender = sender;
    }

    /// <inheritdoc />
    public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        if (command is IRequest<TResult> request)
        {
            return await _sender.Send(request, cancellationToken);
        }

        throw new InvalidOperationException($"Command '{command.GetType().Name}' does not implement MediatR IRequest<{typeof(TResult).Name}>.");
    }

    /// <inheritdoc />
    public async Task<Unit> SendAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        if (command is IRequest<Unit> reqUnit)
        {
            return await _sender.Send(reqUnit, cancellationToken);
        }
        else if (command is IRequest<MediatR.Unit> reqMediatRUnit)
        {
            _ = await _sender.Send(reqMediatRUnit, cancellationToken);
            return Unit.Value;
        }

        throw new InvalidOperationException($"Command '{command.GetType().Name}' does not implement MediatR IRequest.");
    }

    /// <inheritdoc />
    public async Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        if (query is IRequest<TResult> request)
        {
            return await _sender.Send(request, cancellationToken);
        }

        throw new InvalidOperationException($"Query '{query.GetType().Name}' does not implement MediatR IRequest<{typeof(TResult).Name}>.");
    }
}
