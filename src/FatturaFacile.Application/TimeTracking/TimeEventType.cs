// <copyright file="TimeEventType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.TimeTracking;

/// <summary>
/// The time clock event types.
/// </summary>
public static class TimeEventType
{
    /// <summary>
    /// Start of work.
    /// </summary>
    public const string In = "In";

    /// <summary>
    /// End of work.
    /// </summary>
    public const string Out = "Out";

    /// <summary>
    /// Start of a break.
    /// </summary>
    public const string BreakStart = "BreakStart";

    /// <summary>
    /// End of a break.
    /// </summary>
    public const string BreakEnd = "BreakEnd";

    /// <summary>
    /// Gets all the event types.
    /// </summary>
    public static IReadOnlyList<string> All { get; } = [In, Out, BreakStart, BreakEnd];

    /// <summary>
    /// Returns whether the event starts a working period.
    /// </summary>
    /// <param name="eventType">The event type.</param>
    /// <returns>True for In and BreakEnd.</returns>
    public static bool StartsWork(string? eventType) =>
        string.Equals(eventType, In, StringComparison.Ordinal) || string.Equals(eventType, BreakEnd, StringComparison.Ordinal);
}
