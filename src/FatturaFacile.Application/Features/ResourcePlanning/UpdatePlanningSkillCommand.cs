// <copyright file="UpdatePlanningSkillCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Updates a skill of the taxonomy.
/// </summary>
/// <param name="SkillId">The skill.</param>
/// <param name="Skill">The new values.</param>
public sealed record UpdatePlanningSkillCommand(Guid SkillId, PlanningSkillRequest Skill) : IRequest<PlanningCommandOutcome<PlanningSkill>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
