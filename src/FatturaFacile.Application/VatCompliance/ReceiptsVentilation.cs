// <copyright file="ReceiptsVentilation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// The scorporo of daily takings of the register of corrispettivi (art. 24 DPR 633/1972), including the ventilazione of art. 24
/// c. 1 for retailers who cannot tell the rate of each sale.
/// </summary>
public static class ReceiptsVentilation
{
    /// <summary>
    /// The ordinary Italian VAT rate used when there is nothing to ventilate on.
    /// </summary>
    public const decimal OrdinaryRate = 22m;

    private const decimal Hundred = 100m;

    /// <summary>
    /// Splits a gross takings amount into taxable amount and VAT at a single rate.
    /// </summary>
    /// <param name="gross">The gross amount, VAT included.</param>
    /// <param name="rate">The VAT rate in percent.</param>
    /// <returns>The taxable amount and the VAT; together they equal <paramref name="gross"/>.</returns>
    public static (decimal Taxable, decimal Vat) Scorporo(decimal gross, decimal rate)
    {
        var taxable = Math.Round(gross / (1m + (rate / Hundred)), 2, MidpointRounding.AwayFromZero);

        return (taxable, gross - taxable);
    }

    /// <summary>
    /// Ventilates a gross takings amount over the rates, in proportion to the taxable purchases of the same rates: the rule that
    /// lets a retailer without a rate per sale find the VAT due.
    /// </summary>
    /// <param name="gross">The gross takings, VAT included.</param>
    /// <param name="purchasesByRate">The taxable purchases of the reference period by rate.</param>
    /// <returns>The taxable amount and the VAT of each rate; their sum equals <paramref name="gross"/> to the cent.</returns>
    public static IReadOnlyList<(decimal Rate, decimal Taxable, decimal Vat)> Distribute(decimal gross, IReadOnlyList<(decimal Rate, decimal Taxable)> purchasesByRate)
    {
        ArgumentNullException.ThrowIfNull(purchasesByRate);

        var weights = purchasesByRate
            .Where(static p => p.Taxable > 0m)
            .GroupBy(static p => p.Rate)
            .Select(static g => (Rate: g.Key, Weight: g.Sum(static p => p.Taxable)))
            .OrderByDescending(static w => w.Rate)
            .ToList();

        if (weights.Count == 0)
        {
            var (taxable, vat) = Scorporo(gross, OrdinaryRate);

            return [(OrdinaryRate, taxable, vat)];
        }

        var factor = gross / weights.Sum(static w => w.Weight * (1m + (w.Rate / Hundred)));
        var lines = weights.ConvertAll(w => LineOf(w.Rate, factor * w.Weight));

        // The rounding difference goes to the largest line so that the register closes to the takings.
        var difference = gross - lines.Sum(static l => l.Taxable + l.Vat);
        if (difference != 0m)
        {
            var largest = lines.IndexOf(lines.MaxBy(static l => l.Taxable));
            var (rate, taxable, vat) = lines[largest];
            lines[largest] = (rate, taxable + difference, vat);
        }

        return lines;
    }

    private static (decimal Rate, decimal Taxable, decimal Vat) LineOf(decimal rate, decimal exactTaxable)
    {
        var taxable = Math.Round(exactTaxable, 2, MidpointRounding.AwayFromZero);

        return (rate, taxable, Math.Round(taxable * rate / Hundred, 2, MidpointRounding.AwayFromZero));
    }
}
