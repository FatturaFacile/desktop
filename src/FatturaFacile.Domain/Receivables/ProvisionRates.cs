// <copyright file="ProvisionRates.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// The prudential bad-debt provision rates per aging bucket, in percent: 0.5, 1, 5, 10, 25 and 50 by default.
/// </summary>
public static class ProvisionRates
{
    /// <summary>
    /// Gets the default rates, one per bucket in aging order.
    /// </summary>
    public static IReadOnlyList<decimal> Defaults { get; } = [0.5m, 1m, 5m, 10m, 25m, 50m];

    /// <summary>
    /// Gets the default rate of the bucket at a position; positions past the last reuse the last rate.
    /// </summary>
    /// <param name="index">The zero-based bucket position.</param>
    /// <returns>The rate in percent.</returns>
    public static decimal DefaultFor(int index) => Defaults[Math.Clamp(index, 0, Defaults.Count - 1)];

    /// <summary>
    /// Tells whether a rate is acceptable.
    /// </summary>
    /// <param name="rate">The rate in percent.</param>
    /// <returns>True from 0 to 100.</returns>
    public static bool IsValid(decimal rate) => rate is >= 0m and <= 100m;
}
