// <copyright file="SyncStateWriter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Sync;

/// <summary>
/// Maintains the per-record <see cref="SyncEntityState"/> rows.
/// </summary>
internal static class SyncStateWriter
{
    /// <summary>
    /// Creates or updates the state row of a record on the given context (the caller saves).
    /// </summary>
    /// <param name="db">The context.</param>
    /// <param name="existing">The current row, or null when none exists.</param>
    /// <param name="key">The tenant, entity name and record identifier.</param>
    /// <param name="status">The new status.</param>
    /// <param name="serverVersion">The server version, or null to keep the current one.</param>
    /// <param name="now">The current UTC instant.</param>
    internal static void Apply(
        DbContext db,
        SyncEntityState? existing,
        (Guid TenantId, string EntityName, Guid EntityId) key,
        SyncOutboxStatus status,
        string? serverVersion,
        DateTime now)
    {
        var state = existing;
        if (state is null)
        {
            state = new SyncEntityState { TenantId = key.TenantId, EntityName = key.EntityName, EntityId = key.EntityId, CreatedAt = now };
            _ = db.Set<SyncEntityState>().Add(state);
        }

        state.SyncStatus = status;
        state.UpdatedAt = now;
        if (serverVersion is not null)
        {
            state.ServerVersion = serverVersion;
        }

        if (status == SyncOutboxStatus.Synced)
        {
            state.LastSyncedAt = now;
        }
    }
}
