// <copyright file="SaveWbsNodeCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Projects;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Handles <see cref="SaveWbsNodeCommand"/>.
/// </summary>
/// <param name="service">The project portfolio service.</param>
public sealed class SaveWbsNodeCommandHandler(IProjectPortfolioService service) : IRequestHandler<SaveWbsNodeCommand, ProjectActionResult>
{
    /// <inheritdoc/>
    public Task<ProjectActionResult> Handle(SaveWbsNodeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.SaveNodeAsync(request.Node, request.CommandId, cancellationToken);
    }
}
