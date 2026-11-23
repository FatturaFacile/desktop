// <copyright file="ReceivablesMath.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// Monetary arithmetic of the receivables: two decimals, midpoint away from zero, always on <see cref="decimal"/>.
/// </summary>
public static class ReceivablesMath
{
    /// <summary>
    /// The decimals of every amount.
    /// </summary>
    public const int Decimals = 2;

    /// <summary>
    /// Rounds an amount to two decimals.
    /// </summary>
    /// <param name="value">The amount.</param>
    /// <returns>The rounded amount.</returns>
    public static decimal Round(decimal value) => Math.Round(value, Decimals, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Computes a percentage of an amount, rounded to two decimals.
    /// </summary>
    /// <param name="amount">The amount.</param>
    /// <param name="rate">The percentage, 0 to 100.</param>
    /// <returns>The share of the amount.</returns>
    public static decimal Percent(decimal amount, decimal rate) => Round(amount * rate / 100m);
}
