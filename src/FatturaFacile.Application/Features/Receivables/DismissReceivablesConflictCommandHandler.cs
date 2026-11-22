// <copyright file="DismissReceivablesConflictCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="DismissReceivablesConflictCommand"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class DismissReceivablesConflictCommandHandler(IReceivablesService service) : IRequestHandler<DismissReceivablesConflictCommand>
{
    /// <inheritdoc/>
    public Task Handle(DismissReceivablesConflictCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.DismissConflictAsync(request.EntryId, cancellationToken);
    }
}
