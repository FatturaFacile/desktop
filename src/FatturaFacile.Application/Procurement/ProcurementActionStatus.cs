// <copyright file="ProcurementActionStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// How a procurement write ended.
/// </summary>
public enum ProcurementActionStatus
{
    /// <summary>
    /// The server accepted it.
    /// </summary>
    Sent = 0,

    /// <summary>
    /// Saved locally; sent when the server is reachable.
    /// </summary>
    Queued = 1,

    /// <summary>
    /// Refused locally by validation; nothing was saved.
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// The server refused it.
    /// </summary>
    Conflict = 3,
}
