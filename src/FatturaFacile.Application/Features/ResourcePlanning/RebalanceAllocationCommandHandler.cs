// <copyright file="RebalanceAllocationCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="RebalanceAllocationCommand"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class RebalanceAllocationCommandHandler(IResourcePlanningApiClient client) : IRequestHandler<RebalanceAllocationCommand, PlanningCommandOutcome<AllocationMutationResult>>
{
    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<AllocationMutationResult>> Handle(RebalanceAllocationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.RebalanceAsync(request.AllocationId, request.Rebalance, cancellationToken);
    }
}
