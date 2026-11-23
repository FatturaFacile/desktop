// <copyright file="DeletePlanningSkillCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="DeletePlanningSkillCommand"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class DeletePlanningSkillCommandHandler(IResourcePlanningApiClient client) : IRequestHandler<DeletePlanningSkillCommand, PlanningCommandOutcome<bool>>
{
    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<bool>> Handle(DeletePlanningSkillCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.DeleteSkillAsync(request.SkillId, cancellationToken);
    }
}
