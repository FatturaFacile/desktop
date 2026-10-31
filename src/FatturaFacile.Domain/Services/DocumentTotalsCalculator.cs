// <copyright file="DocumentTotalsCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Services;

/// <summary>
/// Domain service responsible for computing financial totals, line subtotals, VAT amounts,
/// and currency-specific rounding adjustments (such as Swiss QR-Bill 0.05 CHF rules) with high precision.
/// </summary>
public class DocumentTotalsCalculator
{
    /// <summary>
    /// Computes the net subtotal, tax breakdown, gross total, and rounding differential for a set of document line items.
    /// </summary>
    /// <param name="items">The collection of line items to evaluate.</param>
    /// <param name="currency">The ISO currency code (defaults to "EUR"). Affects rounding rules (e.g., "CHF").</param>
    /// <returns>A <see cref="DocumentCalculationResult"/> containing detailed line calculations and overall totals.</returns>
    public DocumentCalculationResult Calculate(IEnumerable<DocumentLineInput> items, string currency = "EUR")
    {
        var isChf = string.Equals(currency, "CHF", StringComparison.OrdinalIgnoreCase);
        var totalNet = 0m;
        var totalTax = 0m;
        var computedItems = new List<DocumentLineResult>();

        foreach (var item in items)
        {
            var qty = item.Quantity;
            var price = item.UnitPrice;
            var discount = item.DiscountPercentage;
            var taxRate = item.TaxRate;

            var subtotal = qty * price;
            if (discount > 0m)
            {
                subtotal -= subtotal * (discount / 100.0m);
            }

            var lineNet = Math.Round(subtotal, 2, MidpointRounding.AwayFromZero);
            var lineTax = Math.Round(lineNet * (taxRate / 100.0m), 2, MidpointRounding.AwayFromZero);

            totalNet += lineNet;
            totalTax += lineTax;

            computedItems.Add(new DocumentLineResult(
                item.ProductId,
                item.Description,
                qty,
                price,
                taxRate,
                discount,
                lineNet,
                lineTax));
        }

        var unroundedGross = totalNet + totalTax;
        var totalGross = unroundedGross;
        var roundingAmount = 0m;

        if (isChf)
        {
            // Swiss 5 centimes (0.05 CHF) rounding: round(amount * 20.0) / 20.0
            totalGross = RoundChfAmount(unroundedGross);
            roundingAmount = Math.Round(totalGross - unroundedGross, 2, MidpointRounding.AwayFromZero);
        }

        return new DocumentCalculationResult(
            computedItems,
            Math.Round(totalNet, decimals: 2, MidpointRounding.AwayFromZero),
            Math.Round(totalTax, decimals: 2, MidpointRounding.AwayFromZero),
            Math.Round(totalGross, decimals: 2, MidpointRounding.AwayFromZero),
            roundingAmount);
    }

    /// <summary>
    /// Rounds a monetary gross amount according to the Swiss 5 centimes (0.05 CHF) rounding convention.
    /// Formula: Math.Round(amount * 20.0m) / 20.0m.
    /// </summary>
    /// <param name="amount">The monetary amount to round.</param>
    /// <returns>The amount rounded to the nearest 0.05 unit.</returns>
    public static decimal RoundChfAmount(decimal amount)
    {
        return Math.Round(amount * 20.0m, 0, MidpointRounding.AwayFromZero) / 20.0m;
    }
}
