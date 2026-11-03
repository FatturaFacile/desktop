// <copyright file="DocumentTotalsCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Services;

/// <summary>
/// Domain service responsible for computing financial totals, line subtotals, VAT amounts,
/// and currency-specific rounding adjustments (such as Swiss QR-Bill 0.05 CHF rules) with high precision.
/// </summary>
public static class DocumentTotalsCalculator
{
    /// <summary>
    /// Swiss cash rounding increment (5 centimes).
    /// </summary>
    private const decimal ChfRoundingIncrement = 0.05m;

    /// <summary>
    /// Computes the net subtotal, tax breakdown, gross total, and rounding differential for a set of document line items.
    /// </summary>
    /// <param name="items">The collection of line items to evaluate.</param>
    /// <param name="currency">
    /// The ISO currency code (defaults to EUR). Affects rounding rules such as CHF cash rounding.
    /// </param>
    /// <returns>
    /// A <see cref="DocumentCalculationResult"/> containing detailed line calculations and overall totals.
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
            var quantity = item.Quantity;
            var unitPrice = item.UnitPrice;
            var discountPercentage = item.DiscountPercentage;
            var taxRate = item.TaxRate;

            var subtotal = quantity * unitPrice;

            if (discountPercentage > 0m)
            {
                subtotal -= subtotal * (discountPercentage / 100m);
            }

            var lineNet = Math.Round(
                subtotal,
                2,
                MidpointRounding.AwayFromZero);

            var lineTax = Math.Round(
                lineNet * (taxRate / 100m),
                2,
                MidpointRounding.AwayFromZero);

            totalNet += lineNet;
            totalTax += lineTax;

            computedItems.Add(
                new DocumentLineResult(
                    item.ProductId,
                    item.Description,
                    quantity,
                    unitPrice,
                    taxRate,
                    discountPercentage,
                    lineNet,
                    lineTax));
        }

        var unroundedGross = totalNet + totalTax;
        var totalGross = unroundedGross;
        var roundingAmount = 0m;

        if (isChf)
        {
            totalGross = RoundChfAmount(unroundedGross);

            roundingAmount = Math.Round(
                totalGross - unroundedGross,
                2,
                MidpointRounding.AwayFromZero);
        }

        return new DocumentCalculationResult(
            computedItems,
            Math.Round(totalNet, 2, MidpointRounding.AwayFromZero),
            Math.Round(totalTax, 2, MidpointRounding.AwayFromZero),
            Math.Round(totalGross, 2, MidpointRounding.AwayFromZero),
            roundingAmount);
    }

    /// <summary>
    /// Rounds a monetary amount according to the Swiss 5 centimes (CHF 0.05) cash rounding convention.
    /// </summary>
    /// <param name="amount">The monetary amount to round.</param>
    /// <returns>The amount rounded to the nearest CHF 0.05.</returns>
    public static decimal RoundChfAmount(decimal amount)
    {
        return Math.Round(
                   amount / ChfRoundingIncrement,
                   0,
                   MidpointRounding.AwayFromZero)
               * ChfRoundingIncrement;
    }
}
