// <copyright file="SyncProjectEvmSnapshotCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ProjectEvm;
using MediatR;

namespace FatturaFacile.Application.Features.ProjectEvm;

/// <summary>
/// Handles <see cref="SyncProjectEvmSnapshotCommand"/>.
/// </summary>
/// <param name="sync">The sync service.</param>
public sealed class SyncProjectEvmSnapshotCommandHandler(IProjectEvmSyncService sync) : IRequestHandler<SyncProjectEvmSnapshotCommand, ProjectEvmSyncResult>
{
    /// <inheritdoc/>
    public Task<ProjectEvmSyncResult> Handle(SyncProjectEvmSnapshotCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.ProjectId is null ? sync.SyncProjectsAsync(cancellationToken) : sync.SyncAsync(request.ProjectId, cancellationToken);
    }
}
