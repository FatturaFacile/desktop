// <copyright file="CreatePlanningSkillCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Creates a skill of the taxonomy; the outcome carries the skill or the violations of the server.
/// </summary>
/// <param name="Skill">The skill to create.</param>
public sealed record CreatePlanningSkillCommand(PlanningSkillRequest Skill) : IRequest<PlanningCommandOutcome<PlanningSkill>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
