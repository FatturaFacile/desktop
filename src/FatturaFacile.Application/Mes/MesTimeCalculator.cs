// <copyright file="MesTimeCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Mes;

/// <summary>
/// The time engine of the MES: turns the timestamped intervals of a phase into setup, run and downtime seconds, from which man
/// time and machine time follow. It is the same algorithm as the web-api, so an offline client and the server agree. An open
/// interval runs up to the given instant; an interval that ends before it starts counts zero; durations are whole seconds of
/// UTC timestamps, never client-side counters.
/// </summary>
public static class MesTimeCalculator
{
    /// <summary>
    /// Sums the intervals as of an instant.
    /// </summary>
    /// <param name="intervals">The intervals.</param>
    /// <param name="asOfUtc">The instant open intervals run up to.</param>
    /// <returns>The accumulated times.</returns>
    public static MesTimeSummary Summarize(IEnumerable<MesInterval> intervals, DateTime asOfUtc)
    {
        ArgumentNullException.ThrowIfNull(intervals);

        var summary = MesTimeSummary.Empty;
        foreach (var interval in intervals)
        {
            var open = new MesOpenInterval(interval.Category, interval.StartedAtUtc, interval.MachineAttended, Reason: null);
            summary = summary.Advance(open, Seconds(interval, asOfUtc));
        }

        return summary;
    }

    private static long Seconds(MesInterval interval, DateTime asOfUtc)
    {
        var end = interval.EndedAtUtc ?? asOfUtc;

        return Math.Max(0, (long)Math.Floor((end - interval.StartedAtUtc).TotalSeconds));
    }
}
