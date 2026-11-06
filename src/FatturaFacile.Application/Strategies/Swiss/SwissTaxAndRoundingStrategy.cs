// <copyright file="SwissTaxAndRoundingStrategy.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Strategies.Swiss;

/// <summary>
/// Swiss localization strategy implementing official 0.05 CHF (5 Rappen) commercial rounding.
/// </summary>
public sealed class SwissTaxAndRoundingStrategy : ICountryTaxAndRoundingStrategy
{
    /// <inheritdoc/>
    public string CountryCode => "CH";

    /// <inheritdoc/>
    public string DefaultCurrency => "CHF";

    /// <summary>
    /// Rounds the amount to the nearest 0.05 Swiss Francs (5 Rappen / centimes).
    /// Formula: Math.Round(amount * 20, MidpointRounding.AwayFromZero) / 20.
    /// </summary>
    /// <param name="amount">The raw gross amount.</param>
    /// <returns>The amount rounded to 0.05 CHF.</returns>
    public decimal RoundFinalAmount(decimal amount) => Math.Round(amount * 20m, MidpointRounding.AwayFromZero) / 20m;

    /// <inheritdoc/>
    public decimal CalculateRoundingAdjustment(decimal unroundedAmount, decimal roundedAmount) => roundedAmount - unroundedAmount;

    /// <inheritdoc/>
    public decimal CalculateTax(decimal netAmount, decimal taxRate)
    {
        var rawTax = netAmount * (taxRate / 100m);
        return Math.Round(rawTax, 2, MidpointRounding.AwayFromZero);
    }
}
