// <copyright file="SyncDssSnapshotCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Dss;
using MediatR;

namespace FatturaFacile.Application.Features.Dss;

/// <summary>
/// Handler for <see cref="SyncDssSnapshotCommand"/>.
/// </summary>
/// <param name="sync">The DSS sync service.</param>
public sealed class SyncDssSnapshotCommandHandler(IDssSyncService sync) : IRequestHandler<SyncDssSnapshotCommand, DssSyncResult>
{
    /// <inheritdoc/>
    public Task<DssSyncResult> Handle(SyncDssSnapshotCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return sync.SyncAsync(request.Filter, cancellationToken);
    }
}
