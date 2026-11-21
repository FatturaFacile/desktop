// <copyright file="GetOrderCostingQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Manufacturing.Costing;
using MediatR;

namespace FatturaFacile.Application.Manufacturing.Features;

/// <summary>
/// Handles <see cref="GetOrderCostingQuery"/>.
/// </summary>
/// <param name="store">The offline store.</param>
public sealed class GetOrderCostingQueryHandler(IManufacturingReadStore store) : IRequestHandler<GetOrderCostingQuery, WipCostResult>
{
    /// <inheritdoc/>
    public Task<WipCostResult> Handle(GetOrderCostingQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return CostAsync(request, cancellationToken);
    }

    private async Task<WipCostResult> CostAsync(GetOrderCostingQuery request, CancellationToken cancellationToken)
    {
        var order = await store.GetOrderAsync(request.OrderId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Production order {request.OrderId} is not in the local store.");
        var workbench = await store.GetWorkbenchAsync(cancellationToken).ConfigureAwait(false);
        var logs = await store.GetLogsAsync(order.Id, cancellationToken).ConfigureAwait(false);
        var centers = workbench.WorkCenters.ToDictionary(c => c.Id);

        var standard = OrderCostingService.StandardFor(order, workbench.Boms.ToList(), centers);
        var actual = OrderCostingService.ActualFor(order, logs, centers);
        return WipCostingCalculator.Evaluate(order.TargetQuantity, order.CompletedQuantity, standard, actual, order.Status);
    }
}
