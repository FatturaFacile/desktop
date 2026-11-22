// <copyright file="DeleteWbsNodeCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Projects;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Handles <see cref="DeleteWbsNodeCommand"/>.
/// </summary>
/// <param name="service">The project portfolio service.</param>
public sealed class DeleteWbsNodeCommandHandler(IProjectPortfolioService service) : IRequestHandler<DeleteWbsNodeCommand, ProjectActionResult>
{
    /// <inheritdoc/>
    public Task<ProjectActionResult> Handle(DeleteWbsNodeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.DeleteNodeAsync(request.ProjectId, request.NodeId, request.CommandId, cancellationToken);
    }
}
