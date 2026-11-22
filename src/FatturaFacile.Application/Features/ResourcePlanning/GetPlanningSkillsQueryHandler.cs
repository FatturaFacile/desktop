// <copyright file="GetPlanningSkillsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="GetPlanningSkillsQuery"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class GetPlanningSkillsQueryHandler(IResourcePlanningApiClient client) : IRequestHandler<GetPlanningSkillsQuery, IReadOnlyList<PlanningSkill>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<PlanningSkill>> Handle(GetPlanningSkillsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.GetSkillsAsync(request.Category, cancellationToken);
    }
}
