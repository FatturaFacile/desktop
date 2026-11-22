// <copyright file="GetCapacityHeatmapQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Loads the capacity saturation heatmap of a window.
/// </summary>
/// <param name="From">The first day.</param>
/// <param name="To">The last day.</param>
/// <param name="Granularity">The cell size: week or day.</param>
/// <param name="DepartmentId">The department filter, or null.</param>
public sealed record GetCapacityHeatmapQuery(DateOnly From, DateOnly To, HeatmapGranularity Granularity, Guid? DepartmentId) : IRequest<CapacityHeatmap>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
