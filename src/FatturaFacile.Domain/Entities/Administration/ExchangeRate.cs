// <copyright file="ExchangeRate.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Administration;

/// <summary>
/// Represents a foreign currency exchange conversion rate between base and target currencies for multi-currency transactions and cross-border billing.
/// </summary>
public sealed class ExchangeRate : Entity<Guid>
{
    private ExchangeRate()
    {
    }

    /// <summary>
    /// Gets the base source currency of the rate quote.
    /// </summary>
    public Currency BaseCurrency { get; private set; }

    /// <summary>
    /// Gets the target destination currency resulting from the conversion.
    /// </summary>
    public Currency TargetCurrency { get; private set; }

    /// <summary>
    /// Gets the multiplicative conversion factor applied to an amount in the base currency to determine the value in the target currency.
    /// </summary>
    public decimal Rate { get; private set; }

    /// <summary>
    /// Gets the calendar date on which this exchange rate is officially applicable.
    /// </summary>
    public DateTime EffectiveDate { get; private set; }

    /// <summary>
    /// Gets the institutional market data provider or authority for this rate quote (e.g., "ECB" for European Central Bank or "SNB" for Swiss National Bank).
    /// </summary>
    public string Source { get; private set; } = "ECB";

    /// <summary>
    /// Creates and initializes a new <see cref="ExchangeRate"/> instance.
    /// </summary>
    /// <param name="baseCurr">The base currency being converted from.</param>
    /// <param name="targetCurr">The counter or target currency being converted to.</param>
    /// <param name="rate">The conversion rate factor.</param>
    /// <param name="date">The calendar date on which this rate takes effect.</param>
    /// <param name="source">The reference publishing authority (e.g., "ECB", "SNB"). Defaults to "ECB".</param>
    /// <returns>A configured <see cref="ExchangeRate"/> entity.</returns>
    public static ExchangeRate Create(
        Currency baseCurr,
        Currency targetCurr,
        decimal rate,
        DateTime date,
        string source = "ECB")
    {
        return new ExchangeRate
        {
            Id = Guid.NewGuid(),
            BaseCurrency = baseCurr,
            TargetCurrency = targetCurr,
            Rate = rate,
            EffectiveDate = date,
            Source = source,
        };
    }
}
