// <copyright file="ItalianVatPeriod.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

namespace FatturaFacile.Application.Taxation;

/// <summary>
/// Period helpers for the Italian periodic VAT (LIPE and F24).
/// </summary>
public static class ItalianVatPeriod
{
    /// <summary>
    /// Gets a value indicating whether the period is a single month.
    /// </summary>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <returns><see langword="true"/> for a monthly period.</returns>
    public static bool IsMonthly(DateTime from, DateTime to) => from.Year == to.Year && from.Month == to.Month;

    /// <summary>
    /// Gets the quarter of a day.
    /// </summary>
    /// <param name="date">The day.</param>
    /// <returns>The quarter (1-4).</returns>
    public static int Quarter(DateTime date) => ((date.Month - 1) / 3) + 1;

    /// <summary>
    /// Gets the F24 tax code (codice tributo) of the VAT payment: 6001-6012 monthly, 6031-6033 quarterly; the fourth
    /// quarter of ordinary quarterly taxpayers is paid with the annual balance (6099).
    /// </summary>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <returns>The tax code.</returns>
    public static string F24TaxCode(DateTime from, DateTime to)
    {
        if (IsMonthly(from, to))
        {
            return string.Create(CultureInfo.InvariantCulture, $"60{from.Month:00}");
        }

        var quarter = Quarter(from);
        return quarter == 4 ? "6099" : string.Create(CultureInfo.InvariantCulture, $"603{quarter}");
    }
}
