// <copyright file="SalesDocumentWorkflow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Sales;

/// <summary>
/// Status machines of quotes and sales orders, mirroring the web-api transition tables.
/// </summary>
public static class SalesDocumentWorkflow
{
    private static readonly Dictionary<string, string[]> QuoteTransitions = new(StringComparer.Ordinal)
    {
        [QuoteStatuses.Draft] = [QuoteStatuses.PendingApproval, QuoteStatuses.Approved, QuoteStatuses.Sent, QuoteStatuses.Accepted, QuoteStatuses.Rejected, QuoteStatuses.Expired, QuoteStatuses.Superseded],
        [QuoteStatuses.PendingApproval] = [QuoteStatuses.Approved, QuoteStatuses.Rejected, QuoteStatuses.Superseded],
        [QuoteStatuses.Approved] = [QuoteStatuses.Sent, QuoteStatuses.Accepted, QuoteStatuses.Rejected, QuoteStatuses.Expired, QuoteStatuses.Superseded],
        [QuoteStatuses.Sent] = [QuoteStatuses.Accepted, QuoteStatuses.Rejected, QuoteStatuses.Expired, QuoteStatuses.Superseded],
        [QuoteStatuses.Accepted] = [],
        [QuoteStatuses.Rejected] = [],
        [QuoteStatuses.Expired] = [],
        [QuoteStatuses.Superseded] = [],
    };

    private static readonly Dictionary<string, string[]> OrderTransitions = new(StringComparer.Ordinal)
    {
        [SalesOrderStatuses.Draft] = [SalesOrderStatuses.Confirmed, SalesOrderStatuses.Approved, SalesOrderStatuses.OnHold, SalesOrderStatuses.InProduction, SalesOrderStatuses.InFulfillment, SalesOrderStatuses.Shipped, SalesOrderStatuses.Completed, SalesOrderStatuses.Cancelled, SalesOrderStatuses.Rejected],
        [SalesOrderStatuses.Confirmed] = [SalesOrderStatuses.Approved, SalesOrderStatuses.OnHold, SalesOrderStatuses.InProduction, SalesOrderStatuses.InFulfillment, SalesOrderStatuses.Shipped, SalesOrderStatuses.Completed, SalesOrderStatuses.Cancelled, SalesOrderStatuses.Rejected],
        [SalesOrderStatuses.Approved] = [SalesOrderStatuses.Confirmed, SalesOrderStatuses.OnHold, SalesOrderStatuses.InProduction, SalesOrderStatuses.InFulfillment, SalesOrderStatuses.Shipped, SalesOrderStatuses.Completed, SalesOrderStatuses.Cancelled],
        [SalesOrderStatuses.OnHold] = [SalesOrderStatuses.Confirmed, SalesOrderStatuses.Cancelled, SalesOrderStatuses.Rejected],
        [SalesOrderStatuses.InProduction] = [SalesOrderStatuses.OnHold, SalesOrderStatuses.InFulfillment, SalesOrderStatuses.Shipped, SalesOrderStatuses.Completed, SalesOrderStatuses.Cancelled],
        [SalesOrderStatuses.InFulfillment] = [SalesOrderStatuses.OnHold, SalesOrderStatuses.Shipped, SalesOrderStatuses.Completed, SalesOrderStatuses.Cancelled],
        [SalesOrderStatuses.Shipped] = [SalesOrderStatuses.Completed],
        [SalesOrderStatuses.Completed] = [],
        [SalesOrderStatuses.Cancelled] = [],
        [SalesOrderStatuses.Rejected] = [],
    };

    /// <summary>
    /// Determines whether a quote may move from one status to another.
    /// </summary>
    /// <param name="from">The current status.</param>
    /// <param name="to">The target status.</param>
    /// <returns>True when the transition is allowed.</returns>
    public static bool CanTransitionQuote(string from, string to) => Allows(QuoteTransitions, from, to);

    /// <summary>
    /// Determines whether an order may move from one status to another.
    /// </summary>
    /// <param name="from">The current status.</param>
    /// <param name="to">The target status.</param>
    /// <returns>True when the transition is allowed.</returns>
    public static bool CanTransitionOrder(string from, string to) => Allows(OrderTransitions, from, to);

    /// <summary>
    /// Throws when a quote may not move from one status to another.
    /// </summary>
    /// <param name="from">The current status.</param>
    /// <param name="to">The target status.</param>
    /// <exception cref="InvalidOperationException">The transition is not allowed.</exception>
    public static void AssertQuoteTransition(string from, string to)
    {
        if (!CanTransitionQuote(from, to))
        {
            throw new InvalidOperationException("The quote cannot move from '" + from + "' to '" + to + "'.");
        }
    }

    /// <summary>
    /// Throws when an order may not move from one status to another.
    /// </summary>
    /// <param name="from">The current status.</param>
    /// <param name="to">The target status.</param>
    /// <exception cref="InvalidOperationException">The transition is not allowed.</exception>
    public static void AssertOrderTransition(string from, string to)
    {
        if (!CanTransitionOrder(from, to))
        {
            throw new InvalidOperationException("The order cannot move from '" + from + "' to '" + to + "'.");
        }
    }

    private static bool Allows(Dictionary<string, string[]> table, string from, string to) =>
        table.TryGetValue(from, out var targets) && Array.Exists(targets, t => string.Equals(t, to, StringComparison.Ordinal));
}
