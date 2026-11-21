// <copyright file="CalculateRoutingQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Manufacturing;
using FatturaFacile.Domain.Manufacturing;
using MediatR;

namespace FatturaFacile.Application.Features.Manufacturing;

/// <summary>
/// Handler for <see cref="CalculateRoutingQuery"/>: runs the pure <see cref="RoutingLeadTimeCostCalculator"/> on the cached data.
/// </summary>
/// <param name="store">The local cache.</param>
public sealed class CalculateRoutingQueryHandler(IManufacturingStore store) : IRequestHandler<CalculateRoutingQuery, RoutingCalculation?>
{
    /// <inheritdoc/>
    public Task<RoutingCalculation?> Handle(CalculateRoutingQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return CalculateAsync(request, cancellationToken);
    }

    private async Task<RoutingCalculation?> CalculateAsync(CalculateRoutingQuery request, CancellationToken cancellationToken)
    {
        var overview = await store.GetOverviewAsync(cancellationToken).ConfigureAwait(false);
        var routing = overview.Routings.FirstOrDefault(r => r.Id == request.RoutingId);

        return routing is null || routing.Operations.Count == 0
            ? null
            : RoutingLeadTimeCostCalculator.Calculate(
                routing,
                overview.WorkCenters.ToDictionary(w => w.Id),
                overview.Machines.ToDictionary(m => m.Id),
                request.Quantity,
                request.IncludeOverlap);
    }
}
