// <copyright file="GetCapacityHeatmapQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="GetCapacityHeatmapQuery"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class GetCapacityHeatmapQueryHandler(IResourcePlanningApiClient client) : IRequestHandler<GetCapacityHeatmapQuery, CapacityHeatmap>
{
    /// <inheritdoc/>
    public Task<CapacityHeatmap> Handle(GetCapacityHeatmapQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.GetHeatmapAsync(request.From, request.To, request.Granularity, request.DepartmentId, cancellationToken);
    }
}
