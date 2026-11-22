// <copyright file="RecordVanStockPartConsumptionCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Handles <see cref="RecordVanStockPartConsumptionCommand"/>.
/// </summary>
/// <param name="offline">The offline-first write service.</param>
public sealed class RecordVanStockPartConsumptionCommandHandler(IFieldServiceOfflineService offline) : IRequestHandler<RecordVanStockPartConsumptionCommand, FieldServiceQueueResult>
{
    /// <inheritdoc/>
    public Task<FieldServiceQueueResult> Handle(RecordVanStockPartConsumptionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return offline.QueuePartAsync(request.Part, cancellationToken);
    }
}
