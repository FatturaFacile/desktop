// <copyright file="CreateLocalHolidayCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="CreateLocalHolidayCommand"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class CreateLocalHolidayCommandHandler(IResourcePlanningApiClient client) : IRequestHandler<CreateLocalHolidayCommand, PlanningCommandOutcome<LocalHoliday>>
{
    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<LocalHoliday>> Handle(CreateLocalHolidayCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.CreateLocalHolidayAsync(request.Holiday, cancellationToken);
    }
}
