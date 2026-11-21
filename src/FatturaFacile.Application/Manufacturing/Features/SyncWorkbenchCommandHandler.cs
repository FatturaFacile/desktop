// <copyright file="SyncWorkbenchCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Manufacturing.Sync;
using MediatR;

namespace FatturaFacile.Application.Manufacturing.Features;

/// <summary>
/// Handles <see cref="SyncWorkbenchCommand"/>.
/// </summary>
/// <param name="service">The synchronisation service.</param>
public sealed class SyncWorkbenchCommandHandler(WorkbenchSyncService service) : IRequestHandler<SyncWorkbenchCommand, WorkbenchSyncResult>
{
    /// <inheritdoc/>
    public Task<WorkbenchSyncResult> Handle(SyncWorkbenchCommand request, CancellationToken cancellationToken) =>
        service.SyncAsync(cancellationToken);
}
