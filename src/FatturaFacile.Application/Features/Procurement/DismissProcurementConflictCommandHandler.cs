// <copyright file="DismissProcurementConflictCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Procurement;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Handler for <see cref="DismissProcurementConflictCommand"/>.
/// </summary>
/// <param name="service">The procurement service.</param>
public sealed class DismissProcurementConflictCommandHandler(IProcurementService service) : IRequestHandler<DismissProcurementConflictCommand>
{
    /// <inheritdoc/>
    public Task Handle(DismissProcurementConflictCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.DismissConflictAsync(request.EntryId, cancellationToken);
    }
}
