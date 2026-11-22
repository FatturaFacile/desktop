// <copyright file="InstantiateProjectTemplateCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Projects;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Handles <see cref="InstantiateProjectTemplateCommand"/>.
/// </summary>
/// <param name="service">The project portfolio service.</param>
public sealed class InstantiateProjectTemplateCommandHandler(IProjectPortfolioService service) : IRequestHandler<InstantiateProjectTemplateCommand, ProjectActionResult>
{
    /// <inheritdoc/>
    public Task<ProjectActionResult> Handle(InstantiateProjectTemplateCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.InstantiateTemplateAsync(request.Request, request.CommandId, cancellationToken);
    }
}
