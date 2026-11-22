// <copyright file="SaveProjectCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Projects;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Handles <see cref="SaveProjectCommand"/>.
/// </summary>
/// <param name="service">The project portfolio service.</param>
public sealed class SaveProjectCommandHandler(IProjectPortfolioService service) : IRequestHandler<SaveProjectCommand, ProjectActionResult>
{
    /// <inheritdoc/>
    public Task<ProjectActionResult> Handle(SaveProjectCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.SaveProjectAsync(request.Project, request.CommandId, cancellationToken);
    }
}
