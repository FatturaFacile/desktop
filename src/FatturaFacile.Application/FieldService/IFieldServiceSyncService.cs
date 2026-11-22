// <copyright file="IFieldServiceSyncService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Pulls the Field Service read models from the web-api into the local cache.
/// </summary>
public interface IFieldServiceSyncService
{
    /// <summary>
    /// Pulls one read model; a failure (offline, no session, unreadable answer) leaves the cache untouched.
    /// </summary>
    /// <param name="scope">The read model to pull.</param>
    /// <param name="key">The work order, equipment or technician identifier, for the scopes that need one.</param>
    /// <param name="from">The start of the board window; defaults to the start of the current week.</param>
    /// <param name="to">The end of the board window; defaults to seven days after <paramref name="from"/>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>How the attempt ended.</returns>
    public Task<FieldServiceCallResult> SyncAsync(FieldServiceSyncScope scope, string? key, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken);
}
