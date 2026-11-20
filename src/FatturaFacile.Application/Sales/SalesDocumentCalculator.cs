// <copyright file="SalesDocumentCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Strategies;

namespace FatturaFacile.Application.Sales;

/// <summary>
/// Computes line and document totals of quotes and orders with the country tax and rounding rules.
/// </summary>
public static class SalesDocumentCalculator
{
    /// <summary>
    /// Computes the totals of a document.
    /// </summary>
    /// <param name="lines">The document lines.</param>
    /// <param name="tax">The country tax and rounding strategy.</param>
    /// <returns>The totals.</returns>
    public static SalesTotals Calculate(IReadOnlyCollection<SalesLineDto> lines, ICountryTaxAndRoundingStrategy tax)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(tax);

        List<decimal> lineTotals = [];
        var subtotal = 0m;

        foreach (var line in lines)
        {
            var gross = Math.Round(line.Quantity * line.UnitPrice, 2, MidpointRounding.AwayFromZero);
            subtotal += gross;
            lineTotals.Add(Math.Round(
                line.Quantity * line.UnitPrice * (1m - (line.DiscountPercentage / 100m)),
                2,
                MidpointRounding.AwayFromZero));
        }

        var net = lineTotals.Sum();

        // VAT is computed per rate on the group total (as the VAT return does), not per line.
        var totalTax = lines
            .Select((line, index) => (line.TaxRate, Net: lineTotals[index]))
            .GroupBy(static x => x.TaxRate)
            .Sum(g => tax.CalculateTax(g.Sum(static x => x.Net), g.Key));

        return new SalesTotals(subtotal, subtotal - net, net, totalTax, tax.RoundFinalAmount(net + totalTax), lineTotals);
    }
}
