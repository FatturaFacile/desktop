// <copyright file="GetPlanningHolidaysQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="GetPlanningHolidaysQuery"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class GetPlanningHolidaysQueryHandler(IResourcePlanningApiClient client) : IRequestHandler<GetPlanningHolidaysQuery, IReadOnlyList<PlanningHoliday>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<PlanningHoliday>> Handle(GetPlanningHolidaysQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.GetHolidaysAsync(request.Country, request.Region, request.Municipality, request.Year, cancellationToken);
    }
}
