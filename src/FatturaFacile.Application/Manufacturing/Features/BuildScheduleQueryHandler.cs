// <copyright file="BuildScheduleQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Manufacturing.Scheduling;
using FatturaFacile.Domain.Manufacturing;
using MediatR;

namespace FatturaFacile.Application.Manufacturing.Features;

/// <summary>
/// Handles <see cref="BuildScheduleQuery"/>.
/// </summary>
/// <param name="store">The offline store.</param>
public sealed class BuildScheduleQueryHandler(IManufacturingReadStore store) : IRequestHandler<BuildScheduleQuery, ScheduleResult>
{
    private const decimal MinutesPerHour = 60m;

    /// <inheritdoc/>
    public Task<ScheduleResult> Handle(BuildScheduleQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ScheduleAsync(request, cancellationToken);
    }

    private static decimal Remaining(ProductionOperation operation, decimal target)
    {
        var planned = operation.PlannedSetupMinutes + operation.PlannedRunMinutes;
        var progress = target > 0m ? Math.Min(1m, operation.ConfirmedGoodQuantity / target) : 0m;
        return planned * (1m - progress);
    }

    private async Task<ScheduleResult> ScheduleAsync(BuildScheduleQuery request, CancellationToken cancellationToken)
    {
        var workbench = await store.GetWorkbenchAsync(cancellationToken).ConfigureAwait(false);

        var centers = workbench.WorkCenters
            .Select(c => new WorkCenterCapacity(c.Id, c.Code, c.CapacityHoursPerDay * MinutesPerHour * c.EfficiencyRate * c.AvailabilityRate))
            .Where(c => c.DailyMinutes > 0m)
            .ToList();
        var known = centers.Select(c => c.WorkCenterId).ToHashSet();

        List<SchedulableOrder> orders = [];
        foreach (var order in workbench.Orders.Where(o => o.Status is ProductionOrderStatus.Planned or ProductionOrderStatus.Released or ProductionOrderStatus.InProgress))
        {
            List<SchedulableOperation> operations = [];
            foreach (var operation in order.Operations.Where(op => known.Contains(op.WorkCenterId)))
            {
                var minutes = Remaining(operation, order.TargetQuantity);
                if (minutes > 0m)
                {
                    operations.Add(new SchedulableOperation(operation.Id, operation.Sequence, operation.Name, operation.WorkCenterId, minutes));
                }
            }

            if (operations.Count > 0)
            {
                orders.Add(new SchedulableOrder(order.Id, order.OrderNumber, order.Priority, order.PlannedStartDate, operations));
            }
        }

        return FiniteCapacityScheduler.Schedule(orders, centers, request.HorizonStart);
    }
}
