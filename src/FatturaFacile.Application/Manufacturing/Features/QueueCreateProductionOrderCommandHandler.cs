// <copyright file="QueueCreateProductionOrderCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Manufacturing.Costing;
using FatturaFacile.Application.Manufacturing.Outbox;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Manufacturing;
using MediatR;

namespace FatturaFacile.Application.Manufacturing.Features;

/// <summary>
/// Handles <see cref="QueueCreateProductionOrderCommand"/>.
/// </summary>
/// <param name="store">The offline store.</param>
/// <param name="queue">The outbox queue.</param>
/// <param name="tenant">The current tenant.</param>
/// <param name="timeProvider">The clock.</param>
public sealed class QueueCreateProductionOrderCommandHandler(
    IManufacturingReadStore store,
    OutboxSyncQueue queue,
    ICurrentTenantService tenant,
    TimeProvider timeProvider) : IRequestHandler<QueueCreateProductionOrderCommand, Guid>
{
    private const int StandardDecimals = 2;

    /// <inheritdoc/>
    public Task<Guid> Handle(QueueCreateProductionOrderCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return CreateAsync(request, cancellationToken);
    }

    private async Task<Guid> CreateAsync(QueueCreateProductionOrderCommand request, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        ProductionOrder order = new()
        {
            TenantId = tenant.TenantId,
            OrderNumber = "LOCAL-" + now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            ItemId = request.ItemId,
            ItemCode = request.ItemCode,
            ItemName = request.ItemName,
            BomId = request.BomId,
            RoutingId = request.RoutingId,
            Status = ProductionOrderStatus.Planned,
            TargetQuantity = request.TargetQuantity,
            PlannedStartDate = request.PlannedStartDate.Date,
            PlannedEndDate = request.PlannedEndDate.Date,
            HasPendingChanges = true,
            CreatedAt = now,
        };

        if (request.RoutingId is { } routingId)
        {
            var routing = await store.GetRoutingOperationsAsync(routingId, cancellationToken).ConfigureAwait(false);
            foreach (var step in routing.OrderBy(r => r.Sequence))
            {
                order.Operations.Add(new ProductionOperation
                {
                    TenantId = tenant.TenantId,
                    ProductionOrderId = order.Id,
                    Sequence = step.Sequence,
                    OperationCode = step.OperationCode,
                    Name = step.Name,
                    WorkCenterId = step.WorkCenterId,
                    PlannedSetupMinutes = step.SetupMinutes,
                    PlannedRunMinutes = step.RunMinutesPerUnit * request.TargetQuantity,
                    PlannedLaborMinutes = step.LaborMinutesPerUnit * request.TargetQuantity,
                    CreatedAt = now,
                });
            }
        }

        var workbench = await store.GetWorkbenchAsync(cancellationToken).ConfigureAwait(false);
        var standard = OrderCostingService.StandardFor(order, workbench.Boms.ToList(), workbench.WorkCenters.ToDictionary(c => c.Id)).Round(StandardDecimals);
        order.StandardMaterialCost = standard.Material;
        order.StandardLaborCost = standard.Labor;
        order.StandardOverheadCost = standard.Overhead;

        _ = await queue.QueueCreateOrderAsync(order, cancellationToken).ConfigureAwait(false);
        return order.Id;
    }
}
