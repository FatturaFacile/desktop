// <copyright file="ICountryStrategyFactory.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Strategies;

/// <summary>
/// Factory interface resolving country-specific strategies based on country ISO code or active tenant.
/// </summary>
public interface ICountryStrategyFactory
{
    /// <summary>
    /// Gets the tax and rounding strategy for the given country code.
    /// </summary>
    /// <param name="countryCode">The ISO country code ("CH" or "IT").</param>
    /// <returns>The localized tax and rounding strategy.</returns>
    public ICountryTaxAndRoundingStrategy GetTaxAndRoundingStrategy(string countryCode);

    /// <summary>
    /// Gets the invoicing compliance strategy for the given country code.
    /// </summary>
    /// <param name="countryCode">The ISO country code ("CH" or "IT").</param>
    /// <returns>The localized invoicing compliance strategy.</returns>
    public ICountryInvoicingStrategy GetInvoicingStrategy(string countryCode);

    /// <summary>
    /// Gets the payroll deduction strategy for the given country code.
    /// </summary>
    /// <param name="countryCode">The ISO country code ("CH" or "IT").</param>
    /// <returns>The localized payroll strategy.</returns>
    public ICountryPayrollStrategy GetPayrollStrategy(string countryCode);
}
