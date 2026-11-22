// <copyright file="GetEmployeeSkillProfilesQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Loads the skill profiles of the employees.
/// </summary>
/// <param name="EmployeeId">The employee filter, or null.</param>
/// <param name="SkillId">The skill filter, or null.</param>
public sealed record GetEmployeeSkillProfilesQuery(Guid? EmployeeId, Guid? SkillId) : IRequest<IReadOnlyList<EmployeeSkillProfile>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
