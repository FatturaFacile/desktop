// <copyright file="DsoCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// Days Sales Outstanding: receivables divided by the credit sales of the period times the days of the period.
/// </summary>
public static class DsoCalculator
{
    /// <summary>
    /// The default period in days.
    /// </summary>
    public const int DefaultPeriodDays = 90;

    /// <summary>
    /// The longest period in days.
    /// </summary>
    public const int MaxPeriodDays = 3660;

    /// <summary>
    /// Computes the DSO.
    /// </summary>
    /// <param name="receivables">The open receivables.</param>
    /// <param name="creditSales">The credit sales of the period.</param>
    /// <param name="periodDays">The days of the period, clamped between 1 and 3660.</param>
    /// <returns>The DSO; null when there are receivables but no sales; 0 when there are neither.</returns>
    public static decimal? Calculate(decimal receivables, decimal creditSales, int periodDays = DefaultPeriodDays)
    {
        var days = Math.Clamp(periodDays, 1, MaxPeriodDays);

        return creditSales > 0m ? ReceivablesMath.Round(receivables / creditSales * days) : NoSalesDso(receivables);
    }

    private static decimal? NoSalesDso(decimal receivables) => receivables > 0m ? null : 0m;
}
