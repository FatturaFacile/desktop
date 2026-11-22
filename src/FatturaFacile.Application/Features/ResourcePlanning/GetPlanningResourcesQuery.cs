// <copyright file="GetPlanningResourcesQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Loads the employees that can be planned.
/// </summary>
/// <param name="DepartmentId">The department filter, or null.</param>
public sealed record GetPlanningResourcesQuery(Guid? DepartmentId) : IRequest<IReadOnlyList<PlanningResource>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
