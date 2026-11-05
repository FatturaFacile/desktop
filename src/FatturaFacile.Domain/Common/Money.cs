// <copyright file="Money.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Practical, self-contained record representing a monetary value with its currency.
/// </summary>
/// <param name="Amount">The numerical monetary amount.</param>
/// <param name="Currency">The ISO-4217 three-letter currency code (e.g., EUR, CHF).</param>
public sealed record Money(decimal Amount, string Currency)
{
    /// <summary>
    /// Gets a zero money instance with the specified currency.
    /// </summary>
    /// <param name="currency">The currency code.</param>
    /// <returns>A new <see cref="Money"/> instance with zero amount.</returns>
    public static Money Zero(string currency) => new(0m, currency);
}
