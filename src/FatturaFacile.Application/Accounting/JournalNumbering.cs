// <copyright file="JournalNumbering.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// Sequential entry numbers per year (GLE-2026-0001).
/// </summary>
public static class JournalNumbering
{
    /// <summary>
    /// The prefix of the manual entries.
    /// </summary>
    public const string Prefix = "GLE";

    /// <summary>
    /// Builds the number following the highest existing one of the year.
    /// </summary>
    /// <param name="year">The year of the posting date.</param>
    /// <param name="existingNumbers">The entry numbers already used.</param>
    /// <returns>The next number.</returns>
    public static string Next(int year, IEnumerable<string> existingNumbers)
    {
        ArgumentNullException.ThrowIfNull(existingNumbers);

        var head = string.Create(CultureInfo.InvariantCulture, $"{Prefix}-{year}-");
        var max = 0;
        foreach (var number in existingNumbers)
        {
            if (number.StartsWith(head, StringComparison.Ordinal)
                && int.TryParse(number.AsSpan(head.Length), NumberStyles.None, CultureInfo.InvariantCulture, out var sequence)
                && sequence > max)
            {
                max = sequence;
            }
        }

        return head + (max + 1).ToString("0000", CultureInfo.InvariantCulture);
    }
}
