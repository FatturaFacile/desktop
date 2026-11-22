// <copyright file="DeleteTaskDependencyCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Projects;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Handles <see cref="DeleteTaskDependencyCommand"/>.
/// </summary>
/// <param name="service">The project portfolio service.</param>
public sealed class DeleteTaskDependencyCommandHandler(IProjectPortfolioService service) : IRequestHandler<DeleteTaskDependencyCommand, ProjectActionResult>
{
    /// <inheritdoc/>
    public Task<ProjectActionResult> Handle(DeleteTaskDependencyCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.DeleteDependencyAsync(request.ProjectId, request.DependencyId, request.CommandId, cancellationToken);
    }
}
