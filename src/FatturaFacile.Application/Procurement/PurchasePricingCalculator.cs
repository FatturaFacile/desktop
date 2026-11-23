// <copyright file="PurchasePricingCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// Local estimate of the pricing of a purchase line: cascading discounts, dual unit of measure and company currency. It mirrors the
/// pricing preview of the server and is used when the server cannot be reached; the server stays the authority.
/// </summary>
public static class PurchasePricingCalculator
{
    private const decimal Hundred = 100m;
    private const int PriceDecimals = 4;
    private const int AmountDecimals = 2;

    /// <summary>
    /// Computes the pricing of a line.
    /// </summary>
    /// <param name="request">The inputs.</param>
    /// <returns>The computed amounts.</returns>
    public static PricingPreview Compute(PricingPreviewRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var steps = NormalizeCascade(request.DiscountCascade);
        var factor = request.UomConversionFactor is > 0m ? request.UomConversionFactor.Value : 1m;
        var rate = request.ExchangeRate is > 0m ? request.ExchangeRate.Value : 1m;
        var remaining = steps.Aggregate(1m, (product, step) => product * (1m - (step / Hundred)));
        var netUnit = Round(request.UnitPrice * remaining, PriceDecimals);
        var net = Round(request.Quantity * netUnit, AmountDecimals);
        var tax = Round(net * (request.TaxRate ?? 0m) / Hundred, AmountDecimals);

        return new PricingPreview(
            steps,
            Round((1m - remaining) * Hundred, PriceDecimals),
            netUnit,
            net,
            tax,
            net + tax,
            Round(request.Quantity * factor, PriceDecimals),
            Round(netUnit / factor, PriceDecimals),
            Round(net * rate, AmountDecimals));
    }

    /// <summary>
    /// Keeps the valid steps (0 to 100 percent) of a cascade, in order.
    /// </summary>
    /// <param name="cascade">The steps.</param>
    /// <returns>The valid steps.</returns>
    public static IReadOnlyList<decimal> NormalizeCascade(IReadOnlyList<decimal>? cascade) =>
        cascade is null ? [] : [.. cascade.Where(step => step is >= 0m and <= Hundred)];

    /// <summary>
    /// Parses a cascade typed by the user, for example "10 + 5 + 2" or "10;5;2", with a point or a comma as decimal separator.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="steps">The steps when the text is valid.</param>
    /// <returns>True when every step is a number from 0 to 100.</returns>
    public static bool TryParseCascade(string? text, out IReadOnlyList<decimal> steps)
    {
        List<decimal> parsed = [];
        steps = parsed;

        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        foreach (var part in text.Split(['+', ';', '/', '|', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!decimal.TryParse(part.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var step) || step is < 0m or > Hundred)
            {
                return false;
            }

            parsed.Add(step);
        }

        return true;
    }

    /// <summary>
    /// Formats a cascade for display, for example "10% + 5% + 2%".
    /// </summary>
    /// <param name="cascade">The steps.</param>
    /// <param name="culture">The culture of the user.</param>
    /// <returns>The text, empty when there is no discount.</returns>
    public static string FormatCascade(IReadOnlyList<decimal> cascade, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(cascade);
        ArgumentNullException.ThrowIfNull(culture);

        return string.Join(" + ", cascade.Select(step => step.ToString("0.##", culture) + "%"));
    }

    private static decimal Round(decimal value, int decimals) => Math.Round(value, decimals, MidpointRounding.AwayFromZero);
}
