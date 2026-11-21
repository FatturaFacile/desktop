// <copyright file="MesDurationFormat.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

namespace FatturaFacile.Application.Mes;

/// <summary>
/// Formats the counters of the operator board.
/// </summary>
public static class MesDurationFormat
{
    private const int SecondsPerMinute = 60;
    private const int SecondsPerHour = 3600;

    /// <summary>
    /// Formats a duration as hours:minutes:seconds; the hours are not wrapped at 24, because a phase can run for days.
    /// </summary>
    /// <param name="seconds">The duration in seconds; a negative value counts as zero.</param>
    /// <returns>The text, for example 01:05:09.</returns>
    public static string Format(long seconds)
    {
        var total = Math.Max(0L, seconds);
        var hours = total / SecondsPerHour;
        var inHour = total % SecondsPerHour;
        var minutes = inHour / SecondsPerMinute;
        var rest = total % SecondsPerMinute;

        return string.Create(CultureInfo.InvariantCulture, $"{hours:00}:{minutes:00}:{rest:00}");
    }
}
