// <copyright file="GetCapacitySnapshotsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="GetCapacitySnapshotsQuery"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class GetCapacitySnapshotsQueryHandler(IResourcePlanningApiClient client) : IRequestHandler<GetCapacitySnapshotsQuery, IReadOnlyList<ResourceCapacitySnapshot>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<ResourceCapacitySnapshot>> Handle(GetCapacitySnapshotsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.GetSnapshotsAsync(request.From, request.To, request.EmployeeId, cancellationToken);
    }
}
