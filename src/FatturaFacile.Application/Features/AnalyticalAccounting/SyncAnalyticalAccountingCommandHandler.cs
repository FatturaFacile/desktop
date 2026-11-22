// <copyright file="SyncAnalyticalAccountingCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.AnalyticalAccounting;
using MediatR;

namespace FatturaFacile.Application.Features.AnalyticalAccounting;

/// <summary>
/// Handles <see cref="SyncAnalyticalAccountingCommand"/>.
/// </summary>
/// <param name="sync">The sync service.</param>
public sealed class SyncAnalyticalAccountingCommandHandler(IAnalyticalSyncService sync) : IRequestHandler<SyncAnalyticalAccountingCommand, AnalyticalSyncResult>
{
    /// <inheritdoc/>
    public Task<AnalyticalSyncResult> Handle(SyncAnalyticalAccountingCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return sync.SyncAsync(request.From, request.To, cancellationToken);
    }
}
