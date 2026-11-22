// <copyright file="HelpdeskActionStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// What became of a helpdesk action.
/// </summary>
public enum HelpdeskActionStatus
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
