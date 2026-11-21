// <copyright file="QueueOperationConfirmationCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Manufacturing.Outbox;
using MediatR;

namespace FatturaFacile.Application.Manufacturing.Features;

/// <summary>
/// Handles <see cref="QueueOperationConfirmationCommand"/>.
/// </summary>
/// <param name="queue">The outbox queue.</param>
public sealed class QueueOperationConfirmationCommandHandler(OutboxSyncQueue queue) : IRequestHandler<QueueOperationConfirmationCommand, Guid>
{
    /// <inheritdoc/>
    public Task<Guid> Handle(QueueOperationConfirmationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return queue.QueueConfirmationAsync(request.Input, cancellationToken);
    }
}
