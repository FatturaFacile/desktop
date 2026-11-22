// <copyright file="GetProjectPortfolioQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Projects;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Handles <see cref="GetProjectPortfolioQuery"/>.
/// </summary>
/// <param name="service">The project portfolio service.</param>
public sealed class GetProjectPortfolioQueryHandler(IProjectPortfolioService service) : IRequestHandler<GetProjectPortfolioQuery, ProjectPortfolio>
{
    /// <inheritdoc/>
    public Task<ProjectPortfolio> Handle(GetProjectPortfolioQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadPortfolioAsync(request.Refresh, cancellationToken);
    }
}
