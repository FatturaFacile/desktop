// <copyright file="ProjectOutboxStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// The delivery status of a project outbox entry (Mod 078).
/// </summary>
public enum ProjectOutboxStatus
{
    /// <summary>
    /// Waiting to be sent.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Delivered.
    /// </summary>
    Sent = 1,

    /// <summary>
    /// Refused by the web-api or out of retries.
    /// </summary>
    Conflict = 2,

    /// <summary>
    /// Discarded by the user.
    /// </summary>
    Dismissed = 3,
}
