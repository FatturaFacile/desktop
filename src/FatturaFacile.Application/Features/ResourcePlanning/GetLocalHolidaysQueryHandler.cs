// <copyright file="GetLocalHolidaysQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="GetLocalHolidaysQuery"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class GetLocalHolidaysQueryHandler(IResourcePlanningApiClient client) : IRequestHandler<GetLocalHolidaysQuery, IReadOnlyList<LocalHoliday>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<LocalHoliday>> Handle(GetLocalHolidaysQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.GetLocalHolidaysAsync(cancellationToken);
    }
}
