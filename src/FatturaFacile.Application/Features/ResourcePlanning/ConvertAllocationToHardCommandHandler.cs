// <copyright file="ConvertAllocationToHardCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="ConvertAllocationToHardCommand"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class ConvertAllocationToHardCommandHandler(IResourcePlanningApiClient client) : IRequestHandler<ConvertAllocationToHardCommand, PlanningCommandOutcome<AllocationMutationResult>>
{
    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<AllocationMutationResult>> Handle(ConvertAllocationToHardCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.ConvertToHardAsync(request.AllocationId, request.Conversion, cancellationToken);
    }
}
