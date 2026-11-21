// <copyright file="MesOutboxStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// State of a MES command in the local outbox.
/// </summary>
public enum MesOutboxStatus
{
    /// <summary>
    /// Waiting to be sent, or to be retried.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// The server accepted it (or recognised its idempotency key).
    /// </summary>
    Sent = 1,

    /// <summary>
    /// The server refused it for good (state conflict, validation) or a command it depends on was refused.
    /// </summary>
    Conflict = 2,

    /// <summary>
    /// The operator saw the conflict and dismissed it.
    /// </summary>
    Dismissed = 3,
}
