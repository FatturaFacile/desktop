// <copyright file="GetFieldWorkOrderQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Handles <see cref="GetFieldWorkOrderQuery"/>.
/// </summary>
/// <param name="store">The local cache.</param>
public sealed class GetFieldWorkOrderQueryHandler(IFieldServiceStore store) : IRequestHandler<GetFieldWorkOrderQuery, FieldServiceCached<FieldServiceWorkOrder>?>
{
    /// <inheritdoc/>
    public Task<FieldServiceCached<FieldServiceWorkOrder>?> Handle(GetFieldWorkOrderQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return LoadAsync(request.WorkOrderId, cancellationToken);
    }

    private async Task<FieldServiceCached<FieldServiceWorkOrder>?> LoadAsync(string workOrderId, CancellationToken cancellationToken)
    {
        var cached = await FieldServiceCacheReader.ReadAsync(store, FieldServiceCacheKinds.WorkOrder, workOrderId, FieldServiceWorkOrderParser.ParseDetail, cancellationToken).ConfigureAwait(false);
        if (cached is null)
        {
            return null;
        }

        var pending = await store.PendingAsync(cancellationToken).ConfigureAwait(false);

        return cached with { Value = FieldServiceLocalProjection.Apply(cached.Value, pending) };
    }
}
