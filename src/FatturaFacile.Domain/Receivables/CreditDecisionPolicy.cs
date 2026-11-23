// <copyright file="CreditDecisionPolicy.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// The credit check and the reaction policies, the same rules the web-api enforces: the projected exposure against the limit,
/// the overdue tolerance and the credit hold, then the reaction of the limit to the breach, then the overrides.
/// </summary>
public static class CreditDecisionPolicy
{
    /// <summary>
    /// Evaluates one operation.
    /// </summary>
    /// <param name="exposure">The exposure of the customer.</param>
    /// <param name="limit">The limit in force, when any.</param>
    /// <param name="onHold">Whether the customer is on credit hold.</param>
    /// <param name="scope">The operation.</param>
    /// <param name="additional">What the operation adds to the exposure.</param>
    /// <param name="overrides">The overrides of the customer.</param>
    /// <param name="utcNow">The instant of the check.</param>
    /// <returns>The decision.</returns>
    public static CreditDecision Evaluate(CreditExposure exposure, CustomerCreditLimit? limit, bool onHold, CreditScope scope, decimal additional, IEnumerable<CreditOverride> overrides, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(exposure);
        ArgumentNullException.ThrowIfNull(overrides);

        if (limit is null && !onHold)
        {
            return new CreditDecision(CreditOutcome.Ok, [], Reason: null, 0m, 0m, Overridden: false, ReactionPolicy: null);
        }

        var projected = ReceivablesMath.Round(exposure.TotalExposure + additional);
        var granted = limit?.GrantedLimit ?? 0m;
        List<string> breaches = [];

        if (limit is not null && granted > 0m && projected > granted)
        {
            breaches.Add(CreditBreach.OverLimit);
        }

        if (limit is { MaxOverdueDays: { } maxDays } && exposure.MaxDaysPastDue > maxDays && exposure.OverdueAmount > limit.OverdueToleranceAmount)
        {
            breaches.Add(CreditBreach.OverdueDays);
        }

        if (onHold)
        {
            breaches.Add(CreditBreach.CreditHold);
        }

        if (breaches.Count == 0)
        {
            return new CreditDecision(CreditOutcome.Ok, [], Reason: null, projected, granted, Overridden: false, limit?.ReactionPolicy);
        }

        var policy = limit is null ? CreditReactionPolicy.CreditHold : limit.ReactionPolicy;
        var blocks = Blocks(policy, scope) || (onHold && scope is CreditScope.Order or CreditScope.All);

        var overridden = overrides.Any(candidate => candidate.IsActiveAt(utcNow) && candidate.Covers(scope));

        var outcome = blocks ? CreditOutcome.Block : CreditOutcome.Warning;

        return overridden
            ? new CreditDecision(CreditOutcome.Ok, [.. breaches], Reason: null, projected, granted, Overridden: true, policy)
            : new CreditDecision(outcome, [.. breaches], Reason: null, projected, granted, Overridden: false, policy);
    }

    /// <summary>
    /// Tells whether a reaction policy blocks an operation.
    /// </summary>
    /// <param name="policy">The policy.</param>
    /// <param name="scope">The operation.</param>
    /// <returns>True when the operation is blocked.</returns>
    public static bool Blocks(CreditReactionPolicy policy, CreditScope scope) => policy switch
    {
        CreditReactionPolicy.CreditHold => true,
        CreditReactionPolicy.BlockOrder => scope is CreditScope.Order or CreditScope.All,
        CreditReactionPolicy.BlockDeliveryNote => scope is CreditScope.DeliveryNote or CreditScope.All,
        CreditReactionPolicy.Warning => false,
        _ => false,
    };
}
