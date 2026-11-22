// <copyright file="DeletePlanningSkillCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Deletes a skill of the taxonomy.
/// </summary>
/// <param name="SkillId">The skill.</param>
public sealed record DeletePlanningSkillCommand(Guid SkillId) : IRequest<PlanningCommandOutcome<bool>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
