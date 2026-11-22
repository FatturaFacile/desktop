// <copyright file="CreditDecision.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// The outcome of a credit check for one operation.
/// </summary>
/// <param name="Outcome">Ok, warning or block.</param>
/// <param name="Breaches">The breaches found (see CreditBreach).</param>
/// <param name="Reason">The reason the server gave, when any.</param>
/// <param name="Exposure">The projected exposure.</param>
/// <param name="Limit">The limit in force.</param>
/// <param name="Overridden">Whether an override let the operation through.</param>
/// <param name="ReactionPolicy">The policy of the limit.</param>
public sealed record CreditDecision(
    CreditOutcome Outcome,
    IReadOnlyList<string> Breaches,
    string? Reason,
    decimal Exposure,
    decimal Limit,
    bool Overridden,
    CreditReactionPolicy? ReactionPolicy)
{
    /// <summary>
    /// Gets a value indicating whether the operation is blocked.
    /// </summary>
    public bool Blocks => Outcome == CreditOutcome.Block;

    /// <summary>
    /// Gets a value indicating whether the operation only warns.
    /// </summary>
    public bool Warns => Outcome == CreditOutcome.Warning;
}
