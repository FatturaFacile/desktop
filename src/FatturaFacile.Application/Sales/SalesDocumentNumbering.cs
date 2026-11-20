// <copyright file="SalesDocumentNumbering.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

namespace FatturaFacile.Application.Sales;

/// <summary>
/// Builds the next number of a yearly series such as "QT-2026-0007".
/// </summary>
public static class SalesDocumentNumbering
{
    /// <summary>
    /// Returns the next number of the series for a year.
    /// </summary>
    /// <param name="existingNumbers">The numbers already used (including deleted documents, so numbers are never reused).</param>
    /// <param name="prefix">The series prefix.</param>
    /// <param name="year">The fiscal year.</param>
    /// <returns>The next number.</returns>
    public static string Next(IEnumerable<string> existingNumbers, string prefix, int year)
    {
        ArgumentNullException.ThrowIfNull(existingNumbers);

        var stem = prefix + "-" + year.ToString(CultureInfo.InvariantCulture) + "-";
        var max = 0;

        foreach (var number in existingNumbers)
        {
            if (number.StartsWith(stem, StringComparison.Ordinal)
                && int.TryParse(number.AsSpan(stem.Length), NumberStyles.None, CultureInfo.InvariantCulture, out var sequence)
                && sequence > max)
            {
                max = sequence;
            }
        }

        return stem + (max + 1).ToString("D4", CultureInfo.InvariantCulture);
    }
}
