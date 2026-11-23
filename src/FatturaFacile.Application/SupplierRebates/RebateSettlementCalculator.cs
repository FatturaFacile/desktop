// <copyright file="RebateSettlementCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// Amounts of a settlement proposal: the VAT and gross total of the rebate to settle and the check of an open payable for the
/// set-off. The server stays authoritative; this lets the buyer see the figures before proposing.
/// </summary>
public static class RebateSettlementCalculator
{
    private const decimal Hundred = 100m;

    /// <summary>
    /// Computes the VAT of a rebate amount.
    /// </summary>
    /// <param name="amount">The net rebate.</param>
    /// <param name="taxRatePercent">The VAT rate in percent.</param>
    /// <returns>The VAT, rounded to two decimals.</returns>
    public static decimal Tax(decimal amount, decimal taxRatePercent) => RebateTierCalculator.Round(amount * taxRatePercent / Hundred);

    /// <summary>
    /// Computes the gross total of a rebate amount.
    /// </summary>
    /// <param name="amount">The net rebate.</param>
    /// <param name="taxRatePercent">The VAT rate in percent.</param>
    /// <returns>The net amount plus the VAT.</returns>
    public static decimal Gross(decimal amount, decimal taxRatePercent) => RebateTierCalculator.Round(amount + Tax(amount, taxRatePercent));

    /// <summary>
    /// Checks whether an open payable can absorb a gross rebate.
    /// </summary>
    /// <param name="invoice">The payable.</param>
    /// <param name="gross">The gross amount to set off.</param>
    /// <returns>True when the open balance covers the amount.</returns>
    public static bool CanSetOff(RebatePayableInvoice invoice, decimal gross)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        return invoice.OpenBalance >= gross;
    }
}
