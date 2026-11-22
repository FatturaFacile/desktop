// <copyright file="HelpdeskOutboxStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// The state of a queued helpdesk action.
/// </summary>
public enum HelpdeskOutboxStatus
{
    /// <summary>
    /// Waiting to be sent.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Accepted by the server.
    /// </summary>
    Sent = 1,

    /// <summary>
    /// Refused by the server (or out of retries); waits for the user to dismiss it.
    /// </summary>
    Conflict = 2,

    /// <summary>
    /// Refused and acknowledged by the user.
    /// </summary>
    Dismissed = 3,
}
