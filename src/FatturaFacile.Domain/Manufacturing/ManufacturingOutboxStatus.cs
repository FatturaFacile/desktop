// <copyright file="ManufacturingOutboxStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Manufacturing;

/// <summary>
/// Delivery state of a queued manufacturing mutation.
/// </summary>
public enum ManufacturingOutboxStatus
{
    /// <summary>
    /// Waiting to be sent.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Applied on the server (including idempotent replays).
    /// </summary>
    Synced = 1,

    /// <summary>
    /// Needs a manual decision.
    /// </summary>
    Conflict = 2,

    /// <summary>
    /// Dropped because the server state won.
    /// </summary>
    Discarded = 3,

    /// <summary>
    /// Rejected by the server or out of retries.
    /// </summary>
    Failed = 4,
}
