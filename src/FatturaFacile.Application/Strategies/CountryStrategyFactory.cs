// <copyright file="CountryStrategyFactory.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Strategies.Italian;
using FatturaFacile.Application.Strategies.Swiss;

namespace FatturaFacile.Application.Strategies;

/// <summary>
/// Default implementation of <see cref="ICountryStrategyFactory"/> routing calculations to CH or IT strategies.
/// </summary>
public sealed class CountryStrategyFactory : ICountryStrategyFactory
{
    private readonly SwissTaxAndRoundingStrategy _swissTax = new();
    private readonly ItalianTaxAndRoundingStrategy _italianTax = new();

    private readonly SwissInvoicingStrategy _swissInvoicing = new();
    private readonly ItalianInvoicingStrategy _italianInvoicing = new();

    private readonly SwissPayrollStrategy _swissPayroll = new();
    private readonly ItalianPayrollStrategy _italianPayroll = new();

    /// <inheritdoc/>
    public ICountryTaxAndRoundingStrategy GetTaxAndRoundingStrategy(string countryCode) => IsSwiss(countryCode) ? _swissTax : _italianTax;

    /// <inheritdoc/>
    public ICountryInvoicingStrategy GetInvoicingStrategy(string countryCode) => IsSwiss(countryCode) ? _swissInvoicing : _italianInvoicing;

    /// <inheritdoc/>
    public ICountryPayrollStrategy GetPayrollStrategy(string countryCode) => IsSwiss(countryCode) ? _swissPayroll : _italianPayroll;

    private static bool IsSwiss(string countryCode) => string.Equals(countryCode, "CH", StringComparison.OrdinalIgnoreCase);
}
