// <copyright file="GetProjectPlanQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Projects;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Handles <see cref="GetProjectPlanQuery"/>.
/// </summary>
/// <param name="service">The project portfolio service.</param>
public sealed class GetProjectPlanQueryHandler(IProjectPortfolioService service) : IRequestHandler<GetProjectPlanQuery, ProjectPlan?>
{
    /// <inheritdoc/>
    public Task<ProjectPlan?> Handle(GetProjectPlanQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadPlanAsync(request.ProjectId, cancellationToken);
    }
}
