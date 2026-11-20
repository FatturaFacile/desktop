// <copyright file="SyncOutboxStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Delivery state of an outbox entry (and, by extension, of the record it describes).
/// </summary>
public enum SyncOutboxStatus
{
    /// <summary>
    /// Waiting to be sent, or to be retried after a transient failure.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Accepted by the server.
    /// </summary>
    Synced = 1,

    /// <summary>
    /// The server holds a newer or conflicting version; manual resolution is required.
    /// </summary>
    Conflict = 2,

    /// <summary>
    /// Permanently rejected (validation) or retries exhausted.
    /// </summary>
    Failed = 3,
}
