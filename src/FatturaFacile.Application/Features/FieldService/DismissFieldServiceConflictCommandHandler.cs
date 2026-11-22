// <copyright file="DismissFieldServiceConflictCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Handles <see cref="DismissFieldServiceConflictCommand"/>.
/// </summary>
/// <param name="offline">The offline-first write service.</param>
public sealed class DismissFieldServiceConflictCommandHandler(IFieldServiceOfflineService offline) : IRequestHandler<DismissFieldServiceConflictCommand, Unit>
{
    /// <inheritdoc/>
    public Task<Unit> Handle(DismissFieldServiceConflictCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return DismissAsync(request.EntryId, cancellationToken);
    }

    private async Task<Unit> DismissAsync(Guid entryId, CancellationToken cancellationToken)
    {
        await offline.DismissConflictAsync(entryId, cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
