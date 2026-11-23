// <copyright file="CreditExposure.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// The exposure of a customer: open invoices plus unbilled delivery notes plus confirmed orders minus unreconciled payments.
/// </summary>
/// <param name="OpenInvoices">The open amount of the receivable items.</param>
/// <param name="UnbilledDeliveryNotes">The delivery notes not yet invoiced.</param>
/// <param name="ConfirmedOrders">The confirmed orders not yet delivered.</param>
/// <param name="UnreconciledPayments">The payments on account not yet allocated.</param>
/// <param name="TotalExposure">The total exposure.</param>
/// <param name="OverdueAmount">The open amount that is past due.</param>
/// <param name="OverdueItems">The number of overdue items.</param>
/// <param name="MaxDaysPastDue">The days past due of the oldest overdue item.</param>
public sealed record CreditExposure(
    decimal OpenInvoices,
    decimal UnbilledDeliveryNotes,
    decimal ConfirmedOrders,
    decimal UnreconciledPayments,
    decimal TotalExposure,
    decimal OverdueAmount,
    int OverdueItems,
    int MaxDaysPastDue)
{
    /// <summary>
    /// Gets an exposure of nothing.
    /// </summary>
    public static CreditExposure Empty { get; } = new(0m, 0m, 0m, 0m, 0m, 0m, 0, 0);

    /// <summary>
    /// Applies the exposure rule.
    /// </summary>
    /// <param name="openInvoices">The open invoices.</param>
    /// <param name="unbilledDeliveryNotes">The unbilled delivery notes.</param>
    /// <param name="confirmedOrders">The confirmed orders.</param>
    /// <param name="unreconciledPayments">The unreconciled payments, which reduce the exposure.</param>
    /// <returns>The total exposure.</returns>
    public static decimal Total(decimal openInvoices, decimal unbilledDeliveryNotes, decimal confirmedOrders, decimal unreconciledPayments) =>
        ReceivablesMath.Round(openInvoices + unbilledDeliveryNotes + confirmedOrders - unreconciledPayments);

    /// <summary>
    /// Builds the exposure from the receivable items and the other components.
    /// </summary>
    /// <param name="items">The items of the customer.</param>
    /// <param name="today">The day.</param>
    /// <param name="unbilledDeliveryNotes">The unbilled delivery notes.</param>
    /// <param name="confirmedOrders">The confirmed orders.</param>
    /// <param name="unreconciledPayments">The unreconciled payments.</param>
    /// <returns>The exposure.</returns>
    public static CreditExposure Calculate(IEnumerable<ReceivableItem> items, DateOnly today, decimal unbilledDeliveryNotes, decimal confirmedOrders, decimal unreconciledPayments)
    {
        ArgumentNullException.ThrowIfNull(items);

        var open = 0m;
        var overdue = 0m;
        var overdueItems = 0;
        var maxDays = 0;

        foreach (var item in items.Where(candidate => candidate.IsOpen))
        {
            open += item.OpenAmount;
            var days = item.DaysPastDueOn(today);
            if (days > 0)
            {
                overdue += item.OpenAmount;
                overdueItems++;
                maxDays = Math.Max(maxDays, days);
            }
        }

        return new CreditExposure(
            ReceivablesMath.Round(open),
            ReceivablesMath.Round(unbilledDeliveryNotes),
            ReceivablesMath.Round(confirmedOrders),
            ReceivablesMath.Round(unreconciledPayments),
            Total(open, unbilledDeliveryNotes, confirmedOrders, unreconciledPayments),
            ReceivablesMath.Round(overdue),
            overdueItems,
            maxDays);
    }
}
