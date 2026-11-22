// <copyright file="SaveTaskDependencyCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Projects;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Handles <see cref="SaveTaskDependencyCommand"/>.
/// </summary>
/// <param name="service">The project portfolio service.</param>
public sealed class SaveTaskDependencyCommandHandler(IProjectPortfolioService service) : IRequestHandler<SaveTaskDependencyCommand, ProjectActionResult>
{
    /// <inheritdoc/>
    public Task<ProjectActionResult> Handle(SaveTaskDependencyCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.SaveDependencyAsync(request.Dependency, request.CommandId, cancellationToken);
    }
}
