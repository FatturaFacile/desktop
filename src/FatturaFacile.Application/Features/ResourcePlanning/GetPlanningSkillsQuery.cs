// <copyright file="GetPlanningSkillsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Loads the skills of the taxonomy.
/// </summary>
/// <param name="Category">The category filter, or null.</param>
public sealed record GetPlanningSkillsQuery(string? Category) : IRequest<IReadOnlyList<PlanningSkill>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
