// <copyright file="ReceivablesOutboxStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// The state of a queued receivables action.
/// </summary>
public enum ReceivablesOutboxStatus
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
