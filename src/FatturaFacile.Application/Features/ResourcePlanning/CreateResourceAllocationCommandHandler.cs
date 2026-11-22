// <copyright file="CreateResourceAllocationCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="CreateResourceAllocationCommand"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class CreateResourceAllocationCommandHandler(IResourcePlanningApiClient client) : IRequestHandler<CreateResourceAllocationCommand, PlanningCommandOutcome<AllocationMutationResult>>
{
    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<AllocationMutationResult>> Handle(CreateResourceAllocationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.CreateAllocationAsync(request.Allocation, cancellationToken);
    }
}
