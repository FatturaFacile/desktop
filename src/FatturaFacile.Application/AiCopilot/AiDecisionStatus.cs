// <copyright file="AiDecisionStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AiCopilot;

/// <summary>
/// Outcome of a user decision.
/// </summary>
public enum AiDecisionStatus
{
    /// <summary>
    /// The server accepted the decision.
    /// </summary>
    Sent = 0,

    /// <summary>
    /// The server is unreachable: the decision is queued and will be pushed later.
    /// </summary>
    Queued = 1,

    /// <summary>
    /// The suggestion was already decided elsewhere and was removed from the queue.
    /// </summary>
    AlreadyDecided = 2,

    /// <summary>
    /// The proposal is incomplete (422): it stays pending and the detail explains why.
    /// </summary>
    Incomplete = 3,

    /// <summary>
    /// The suggestion is unknown to the server or to the local cache.
    /// </summary>
    NotFound = 4,

    /// <summary>
    /// The server refused the decision (for example 403); the suggestion stays pending.
    /// </summary>
    Refused = 5,
}
