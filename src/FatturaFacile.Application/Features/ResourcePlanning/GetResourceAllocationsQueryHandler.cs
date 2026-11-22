// <copyright file="GetResourceAllocationsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="GetResourceAllocationsQuery"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class GetResourceAllocationsQueryHandler(IResourcePlanningApiClient client) : IRequestHandler<GetResourceAllocationsQuery, IReadOnlyList<ResourceAllocation>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<ResourceAllocation>> Handle(GetResourceAllocationsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.GetAllocationsAsync(request.From, request.To, request.EmployeeId, request.ProjectId, request.BookingType, cancellationToken);
    }
}
