// <copyright file="DssWaterfallBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Dss;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Builds the stacked-column bars of a variance bridge (a waterfall) from its steps.
/// </summary>
internal static class DssWaterfallBuilder
{
    /// <summary>
    /// Turns the steps of a bridge into bars.
    /// </summary>
    /// <param name="steps">The steps, baseline first and actual last.</param>
    /// <param name="label">Maps a step key to its label.</param>
    /// <returns>One bar per step.</returns>
    public static IReadOnlyList<DssWaterfallBar> Build(IReadOnlyList<DssBridgeStep> steps, Func<string, string> label)
    {
        ArgumentNullException.ThrowIfNull(steps);
        ArgumentNullException.ThrowIfNull(label);

        List<DssWaterfallBar> bars = [];
        var previous = 0.0;

        foreach (var step in steps)
        {
            var name = label(step.Key);
            if (step.IsTotal)
            {
                previous = step.Cumulative;
                bars.Add(new DssWaterfallBar(name, 0, Math.Max(step.Value, 0), Math.Min(step.Value, 0), 0, 0, 0, 0));
                continue;
            }

            var low = Math.Min(previous, step.Cumulative);
            var high = Math.Max(previous, step.Cumulative);
            previous = step.Cumulative;
            var (baseline, positive, negative) = Parts(low, high);

            bars.Add(step.Value >= 0
                ? new DssWaterfallBar(name, baseline, 0, 0, positive, negative, 0, 0)
                : new DssWaterfallBar(name, baseline, 0, 0, 0, 0, positive, negative));
        }

        return bars;
    }

    private static (double Base, double Positive, double Negative) Parts(double low, double high)
    {
        if (low >= 0)
        {
            return (low, high - low, 0);
        }

        // Entirely below zero: the base is the upper end and the bar hangs below it. Across zero: both parts start at zero.
        return high <= 0 ? (high, 0, low - high) : (0, high, low);
    }
}
