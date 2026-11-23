// <copyright file="DismissProjectConflictCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Projects;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Handles <see cref="DismissProjectConflictCommand"/>.
/// </summary>
/// <param name="service">The project portfolio service.</param>
public sealed class DismissProjectConflictCommandHandler(IProjectPortfolioService service) : IRequestHandler<DismissProjectConflictCommand>
{
    /// <inheritdoc/>
    public Task Handle(DismissProjectConflictCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.DismissConflictAsync(request.EntryId, cancellationToken);
    }
}
