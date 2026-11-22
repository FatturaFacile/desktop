// <copyright file="GetPlanningResourcesQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="GetPlanningResourcesQuery"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class GetPlanningResourcesQueryHandler(IResourcePlanningApiClient client) : IRequestHandler<GetPlanningResourcesQuery, IReadOnlyList<PlanningResource>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<PlanningResource>> Handle(GetPlanningResourcesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.GetResourcesAsync(request.DepartmentId, cancellationToken);
    }
}
