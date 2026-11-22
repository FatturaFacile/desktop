// <copyright file="HelpdeskBreachTarget.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// One SLA target of a ticket that is at risk or already breached.
/// </summary>
/// <param name="Target">The target: response, next_update or resolution.</param>
/// <param name="DueAt">The instant the target is due.</param>
/// <param name="ElapsedPercent">The share of the target time that has elapsed.</param>
/// <param name="State">The state: at_risk or breached.</param>
public sealed record HelpdeskBreachTarget(string Target, DateTime? DueAt, double ElapsedPercent, string State)
{
    /// <summary>
    /// The state of a target that is past its due time.
    /// </summary>
    public const string Breached = "breached";

    /// <summary>
    /// The state of a target that is close to its due time.
    /// </summary>
    public const string AtRisk = "at_risk";

    /// <summary>
    /// Gets a value indicating whether the target is breached.
    /// </summary>
    public bool IsBreached => string.Equals(State, Breached, StringComparison.Ordinal);
}
