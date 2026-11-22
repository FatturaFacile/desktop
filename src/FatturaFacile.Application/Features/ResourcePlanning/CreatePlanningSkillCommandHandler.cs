// <copyright file="CreatePlanningSkillCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="CreatePlanningSkillCommand"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class CreatePlanningSkillCommandHandler(IResourcePlanningApiClient client) : IRequestHandler<CreatePlanningSkillCommand, PlanningCommandOutcome<PlanningSkill>>
{
    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<PlanningSkill>> Handle(CreatePlanningSkillCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.CreateSkillAsync(request.Skill, cancellationToken);
    }
}
