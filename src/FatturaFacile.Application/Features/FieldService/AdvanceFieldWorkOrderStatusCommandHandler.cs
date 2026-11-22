// <copyright file="AdvanceFieldWorkOrderStatusCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Handles <see cref="AdvanceFieldWorkOrderStatusCommand"/>.
/// </summary>
/// <param name="offline">The offline-first write service.</param>
public sealed class AdvanceFieldWorkOrderStatusCommandHandler(IFieldServiceOfflineService offline) : IRequestHandler<AdvanceFieldWorkOrderStatusCommand, FieldServiceQueueResult>
{
    /// <inheritdoc/>
    public Task<FieldServiceQueueResult> Handle(AdvanceFieldWorkOrderStatusCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return offline.QueueStatusAsync(request.WorkOrderId, request.Status, cancellationToken);
    }
}
