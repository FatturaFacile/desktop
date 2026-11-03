// <copyright file="DocumentTotalsCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Services;

/// <summary>
/// Domain service responsible for computing financial totals, line subtotals,
/// VAT amounts and currency-specific rounding adjustments
/// (such as Swiss QR-Bill CHF 0.05 rounding rules).
/// </summary>
public static class DocumentTotalsCalculator
{
    /// <summary>
    /// Swiss cash rounding increment (5 centimes).
    /// </summary>
    private const decimal ChfRoundingIncrement = 0.05m;

    /// <summary>
    /// Divisor used to convert percentage values into decimal fractions.
    /// </summary>
    private const decimal PercentageDivisor = 100m;

    /// <summary>
    /// Number of decimal places used for monetary values.
    /// </summary>
    private const int CurrencyDecimals = 2;

    /// <summary>
    /// Computes the net subtotal, tax breakdown, gross total and rounding
    /// differential for a set of document line items.
    /// </summary>
    /// <param name="items">The collection of line items to evaluate.</param>
    /// <param name="currency">The ISO currency code. Defaults to EUR.</param>
    /// <returns>
    /// A <see cref="DocumentCalculationResult"/> containing line calculations
    /// and document totals.
    /// </returns>
    public static DocumentCalculationResult Calculate(
        IEnumerable<DocumentLineInput> items,
        string currency = "EUR")
    {
        ArgumentNullException.ThrowIfNull(items);

        var isChf = string.Equals(
            currency,
            "CHF",
            StringComparison.OrdinalIgnoreCase);

        var totalNet = 0m;
        var totalTax = 0m;

        var computedItems = new List<DocumentLineResult>();

        foreach (var item in items)
        {
            var subtotal = item.Quantity * item.UnitPrice;

            if (item.DiscountPercentage > 0m)
            {
                subtotal -= subtotal * (item.DiscountPercentage / PercentageDivisor);
            }

            var lineNet = RoundCurrency(subtotal);

            var lineTax = RoundCurrency(
                lineNet * (item.TaxRate / PercentageDivisor));

            totalNet += lineNet;
            totalTax += lineTax;

            computedItems.Add(
                new DocumentLineResult(
                    item.ProductId,
                    item.Description,
                    item.Quantity,
                    item.UnitPrice,
                    item.TaxRate,
                    item.DiscountPercentage,
                    lineNet,
                    lineTax));
        }

        var unroundedGross = totalNet + totalTax;
        var totalGross = unroundedGross;
        var roundingAmount = 0m;

        if (isChf)
        {
            totalGross = RoundChfAmount(unroundedGross);

            roundingAmount = RoundCurrency(
                totalGross - unroundedGross);
        }

        return new DocumentCalculationResult(
            computedItems,
            RoundCurrency(totalNet),
            RoundCurrency(totalTax),
            RoundCurrency(totalGross),
            roundingAmount);
    }

    /// <summary>
    /// Rounds a monetary amount using the standard currency precision.
    /// </summary>
    /// <param name="amount">Amount to round.</param>
    /// <returns>The rounded amount.</returns>
    public static decimal RoundCurrency(decimal amount)
    {
        return Math.Round(
            amount,
            CurrencyDecimals,
            MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Rounds a monetary amount according to the Swiss CHF 0.05 cash-rounding convention.
    /// </summary>
    /// <param name="amount">Amount to round.</param>
    /// <returns>The rounded amount.</returns>
    public static decimal RoundChfAmount(decimal amount)
    {
        return Math.Round(
                   amount / ChfRoundingIncrement,
                   0,
                   MidpointRounding.AwayFromZero)
               * ChfRoundingIncrement;
    }
}
