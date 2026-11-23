// <copyright file="UpdatePlanningSkillCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="UpdatePlanningSkillCommand"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class UpdatePlanningSkillCommandHandler(IResourcePlanningApiClient client) : IRequestHandler<UpdatePlanningSkillCommand, PlanningCommandOutcome<PlanningSkill>>
{
    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<PlanningSkill>> Handle(UpdatePlanningSkillCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.UpdateSkillAsync(request.SkillId, request.Skill, cancellationToken);
    }
}
