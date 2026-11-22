// <copyright file="DeleteLocalHolidayCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="DeleteLocalHolidayCommand"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class DeleteLocalHolidayCommandHandler(IResourcePlanningApiClient client) : IRequestHandler<DeleteLocalHolidayCommand, PlanningCommandOutcome<bool>>
{
    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<bool>> Handle(DeleteLocalHolidayCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.DeleteLocalHolidayAsync(request.HolidayId, cancellationToken);
    }
}
