// <copyright file="FlushProjectOutboxCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Projects;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Handles <see cref="FlushProjectOutboxCommand"/>.
/// </summary>
/// <param name="service">The project portfolio service.</param>
public sealed class FlushProjectOutboxCommandHandler(IProjectPortfolioService service) : IRequestHandler<FlushProjectOutboxCommand, ProjectFlushResult>
{
    /// <inheritdoc/>
    public Task<ProjectFlushResult> Handle(FlushProjectOutboxCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.FlushAsync(cancellationToken);
    }
}
