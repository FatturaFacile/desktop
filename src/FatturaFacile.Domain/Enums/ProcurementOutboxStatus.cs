// <copyright file="ProcurementOutboxStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// The state of a queued procurement write.
/// </summary>
public enum ProcurementOutboxStatus
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
    /// Refused by the server; waits for the user to acknowledge it.
    /// </summary>
    Conflict = 2,

    /// <summary>
    /// Acknowledged by the user.
    /// </summary>
    Dismissed = 3,
}
