// <copyright file="GetPlanningHolidaysQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Loads the holidays the planning resolves for a place and a year: national, cantonal or regional, municipal and the local holidays of the tenant.
/// </summary>
/// <param name="Country">The country code, CH or IT.</param>
/// <param name="Region">The canton or region, or null.</param>
/// <param name="Municipality">The municipality, or null.</param>
/// <param name="Year">The year.</param>
public sealed record GetPlanningHolidaysQuery(string Country, string? Region, string? Municipality, int Year) : IRequest<IReadOnlyList<PlanningHoliday>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
