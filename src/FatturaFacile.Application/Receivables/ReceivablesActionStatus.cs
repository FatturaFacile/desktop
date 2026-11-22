// <copyright file="ReceivablesActionStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// What became of a receivables action.
/// </summary>
public enum ReceivablesActionStatus
{
    /// <summary>
    /// The server accepted it.
    /// </summary>
    Sent = 0,

    /// <summary>
    /// It was saved locally and is sent when the server is reachable.
    /// </summary>
    Queued = 1,

    /// <summary>
    /// Local validation refused it; nothing was saved.
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// The server refused it.
    /// </summary>
    Conflict = 3,
}
