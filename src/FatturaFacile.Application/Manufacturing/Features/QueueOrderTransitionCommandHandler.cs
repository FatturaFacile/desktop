// <copyright file="QueueOrderTransitionCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Manufacturing.Outbox;
using MediatR;

namespace FatturaFacile.Application.Manufacturing.Features;

/// <summary>
/// Handles <see cref="QueueOrderTransitionCommand"/>.
/// </summary>
/// <param name="queue">The outbox queue.</param>
public sealed class QueueOrderTransitionCommandHandler(OutboxSyncQueue queue) : IRequestHandler<QueueOrderTransitionCommand, Guid>
{
    /// <inheritdoc/>
    public Task<Guid> Handle(QueueOrderTransitionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return QueueAsync(request, cancellationToken);
    }

    private async Task<Guid> QueueAsync(QueueOrderTransitionCommand request, CancellationToken cancellationToken)
    {
        var entry = await queue.QueueTransitionAsync(request.OrderId, request.Kind, cancellationToken).ConfigureAwait(false);
        return entry.Id;
    }
}
