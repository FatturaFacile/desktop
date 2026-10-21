// <copyright file="ExchangeRate.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Administration;

/// <summary>
/// Represents exchange rate.
/// </summary>
public sealed class ExchangeRate : Entity<Guid>
{
    private ExchangeRate()
    {
    }

    /// <summary>
    /// Gets the base currency.
    /// </summary>
    public Currency BaseCurrency { get; private set; }

    /// <summary>
    /// Gets the target currency.
    /// </summary>
    public Currency TargetCurrency { get; private set; }

    /// <summary>
    /// Gets the rate.
    /// </summary>
    public decimal Rate { get; private set; }

    /// <summary>
    /// Gets the effective date.
    /// </summary>
    public DateTime EffectiveDate { get; private set; }

    /// <summary>
    /// Gets the eCB (European Central Bank) or SNB (Swiss National Bank).
    /// </summary>
    public string Source { get; private set; } = "ECB";

    /// <summary>
    /// Create.
    /// </summary>
    /// <param name="baseCurr">The base curr.</param>
    /// <param name="targetCurr">The target curr.</param>
    /// <param name="rate">The rate.</param>
    /// <param name="date">The date.</param>
    /// <param name="source">The source.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
