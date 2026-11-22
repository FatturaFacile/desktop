// <copyright file="PullProjectPortfolioCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Projects;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Handles <see cref="PullProjectPortfolioCommand"/>.
/// </summary>
/// <param name="service">The project portfolio service.</param>
public sealed class PullProjectPortfolioCommandHandler(IProjectPortfolioService service) : IRequestHandler<PullProjectPortfolioCommand, ProjectPullResult>
{
    /// <inheritdoc/>
    public Task<ProjectPullResult> Handle(PullProjectPortfolioCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.PullAsync(cancellationToken);
    }
}
