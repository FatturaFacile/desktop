// <copyright file="ItalianTaxAndRoundingStrategy.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Strategies.Italian;

/// <summary>
/// Italian localization strategy implementing standard 0.01 EUR currency rounding.
/// </summary>
public sealed class ItalianTaxAndRoundingStrategy : ICountryTaxAndRoundingStrategy
{
    /// <inheritdoc/>
    public string CountryCode => "IT";

    /// <inheritdoc/>
    public string DefaultCurrency => "EUR";

    /// <inheritdoc/>
    public decimal RoundFinalAmount(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);

    /// <inheritdoc/>
    public decimal CalculateRoundingAdjustment(decimal unroundedAmount, decimal roundedAmount) => roundedAmount - unroundedAmount;

    /// <inheritdoc/>
    public decimal CalculateTax(decimal netAmount, decimal taxRate)
    {
        var rawTax = netAmount * (taxRate / 100m);
        return Math.Round(rawTax, 2, MidpointRounding.AwayFromZero);
    }
}
