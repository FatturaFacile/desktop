// <copyright file="CaptureProjectBaselineCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Projects;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Handles <see cref="CaptureProjectBaselineCommand"/>.
/// </summary>
/// <param name="service">The project portfolio service.</param>
public sealed class CaptureProjectBaselineCommandHandler(IProjectPortfolioService service) : IRequestHandler<CaptureProjectBaselineCommand, ProjectActionResult>
{
    /// <inheritdoc/>
    public Task<ProjectActionResult> Handle(CaptureProjectBaselineCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.CaptureBaselineAsync(request.ProjectId, request.Name, request.CommandId, cancellationToken);
    }
}
