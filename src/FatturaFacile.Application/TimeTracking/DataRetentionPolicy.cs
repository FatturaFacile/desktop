// <copyright file="DataRetentionPolicy.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.TimeTracking;

/// <summary>
/// Retention of the working time records: art. 73 cpv. 2 OLL 1 requires them to be kept for at least five years.
/// </summary>
public static class DataRetentionPolicy
{
    /// <summary>
    /// The minimum number of years required by law.
    /// </summary>
    public const int MinimumYears = 5;

    /// <summary>
    /// The maximum number of years that can be configured.
    /// </summary>
    public const int MaximumYears = 30;

    /// <summary>
    /// Gets the number of years actually applied (never less than the legal minimum).
    /// </summary>
    /// <param name="configuredYears">The configured number of years.</param>
    /// <returns>The effective number of years.</returns>
    public static int EffectiveYears(int configuredYears) => Math.Clamp(configuredYears, MinimumYears, MaximumYears);

    /// <summary>
    /// Gets the first day that must still be kept: records of whole calendar years before it have expired.
    /// </summary>
    /// <param name="today">The current day.</param>
    /// <param name="configuredYears">The configured number of years.</param>
    /// <returns>The cutoff day.</returns>
    public static DateTime Cutoff(DateTime today, int configuredYears) =>
        new(today.Year - EffectiveYears(configuredYears), 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
}
