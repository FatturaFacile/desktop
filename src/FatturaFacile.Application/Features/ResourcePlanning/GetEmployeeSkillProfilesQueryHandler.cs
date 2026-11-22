// <copyright file="GetEmployeeSkillProfilesQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="GetEmployeeSkillProfilesQuery"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class GetEmployeeSkillProfilesQueryHandler(IResourcePlanningApiClient client) : IRequestHandler<GetEmployeeSkillProfilesQuery, IReadOnlyList<EmployeeSkillProfile>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<EmployeeSkillProfile>> Handle(GetEmployeeSkillProfilesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.GetSkillProfilesAsync(request.EmployeeId, request.SkillId, cancellationToken);
    }
}
