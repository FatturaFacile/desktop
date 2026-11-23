// <copyright file="RecalculateCapacitySnapshotsCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="RecalculateCapacitySnapshotsCommand"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class RecalculateCapacitySnapshotsCommandHandler(IResourcePlanningApiClient client) : IRequestHandler<RecalculateCapacitySnapshotsCommand, PlanningCommandOutcome<SnapshotRecalculationResult>>
{
    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<SnapshotRecalculationResult>> Handle(RecalculateCapacitySnapshotsCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.RecalculateSnapshotsAsync(request.Recalculation, cancellationToken);
    }
}
