// <copyright file="SyncManufacturingCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Manufacturing;
using MediatR;

namespace FatturaFacile.Application.Features.Manufacturing;

/// <summary>
/// Handler for <see cref="SyncManufacturingCommand"/>.
/// </summary>
/// <param name="sync">The sync service.</param>
public sealed class SyncManufacturingCommandHandler(IManufacturingSyncService sync) : IRequestHandler<SyncManufacturingCommand, ManufacturingSyncResult>
{
    /// <inheritdoc/>
    public Task<ManufacturingSyncResult> Handle(SyncManufacturingCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return sync.SyncAsync(cancellationToken);
    }
}
