// <copyright file="ICountryTaxAndRoundingStrategy.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Strategies;

/// <summary>
/// Strategy contract for jurisdiction-specific rounding rules and tax calculations.
/// </summary>
public interface ICountryTaxAndRoundingStrategy
{
    /// <summary>
    /// Gets the ISO country code this strategy handles ("CH" or "IT").
    /// </summary>
    public string CountryCode { get; }

    /// <summary>
    /// Gets the official currency code ("CHF" or "EUR").
    /// </summary>
    public string DefaultCurrency { get; }

    /// <summary>
    /// Applies jurisdiction-specific rounding to a final monetary amount.
    /// (e.g., Swiss 0.05 Rappen rounding vs Italian 0.01 standard rounding).
    /// </summary>
    /// <param name="amount">The raw gross amount.</param>
    /// <returns>The rounded amount.</returns>
    public decimal RoundFinalAmount(decimal amount);

    /// <summary>
    /// Computes the rounding difference (rounding amount) introduced by the rounding strategy.
    /// </summary>
    /// <param name="unroundedAmount">The exact calculated gross amount.</param>
    /// <param name="roundedAmount">The rounded gross amount.</param>
    /// <returns>The adjustment difference.</returns>
    public decimal CalculateRoundingAdjustment(decimal unroundedAmount, decimal roundedAmount);

    /// <summary>
    /// Computes VAT tax amount for a taxable line or total.
    /// </summary>
    /// <param name="netAmount">The net taxable amount.</param>
    /// <param name="taxRate">The percentage rate.</param>
    /// <returns>The computed tax amount.</returns>
    public decimal CalculateTax(decimal netAmount, decimal taxRate);
}
