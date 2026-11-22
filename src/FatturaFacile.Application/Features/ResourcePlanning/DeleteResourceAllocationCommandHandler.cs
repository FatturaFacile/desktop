// <copyright file="DeleteResourceAllocationCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="DeleteResourceAllocationCommand"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class DeleteResourceAllocationCommandHandler(IResourcePlanningApiClient client) : IRequestHandler<DeleteResourceAllocationCommand, PlanningCommandOutcome<bool>>
{
    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<bool>> Handle(DeleteResourceAllocationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.DeleteAllocationAsync(request.AllocationId, cancellationToken);
    }
}
