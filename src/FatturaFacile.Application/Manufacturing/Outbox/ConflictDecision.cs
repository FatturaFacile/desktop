// <copyright file="ConflictDecision.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing.Outbox;

/// <summary>
/// What to do with an outbox entry the server answered with a conflict.
/// </summary>
public enum ConflictDecision
{
    /// <summary>
    /// The server already reflects the intended state: mark the entry synced.
    /// </summary>
    AlreadyApplied = 0,

    /// <summary>
    /// The server state allows the mutation now: send it again on the next flush.
    /// </summary>
    Reapply = 1,

    /// <summary>
    /// The server state wins: drop the local mutation.
    /// </summary>
    ServerWins = 2,

    /// <summary>
    /// The case is ambiguous: block the order until a person decides.
    /// </summary>
    ManualReview = 3,
}
