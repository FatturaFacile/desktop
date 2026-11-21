// <copyright file="OeeCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Domain.Manufacturing;

/// <summary>
/// Pure port of the web-api OeeCalculator formulas: availability, performance and quality of a period from the OEE logs, the six big
/// losses, the ideal cycle time fallback and the machine telemetry status.
/// </summary>
public static class OeeCalculator
{
    /// <summary>
    /// Minutes of silence after which the telemetry of a machine counts as stale.
    /// </summary>
    public const int StaleAfterMinutes = 15;

    /// <summary>
    /// The loss category that counts the quality losses of a start-up.
    /// </summary>
    public const string StartupRejects = "startup_rejects";

    private const string ProcessDefects = "process_defects";
    private const string EquipmentFailure = "equipment_failure";

    private static readonly string[] Categories =
    [
        EquipmentFailure,
        "setup_adjustment",
        "minor_stops",
        "reduced_speed",
        ProcessDefects,
        StartupRejects,
    ];

    /// <summary>
    /// Calculates the OEE figures of a period.
    /// </summary>
    /// <param name="scheduledMinutes">The minutes the calendar schedules in the period (or the elapsed minutes without calendar).</param>
    /// <param name="logs">The OEE logs of the period.</param>
    /// <param name="idealCycleMinutes">The ideal cycle time in minutes per unit.</param>
    /// <returns>The metrics, with the ratios clamped to 0..1.</returns>
    public static OeeMetrics Calculate(double scheduledMinutes, IEnumerable<OeeLogEntry> logs, double idealCycleMinutes)
    {
        ArgumentNullException.ThrowIfNull(logs);

        var list = logs as IReadOnlyCollection<OeeLogEntry> ?? [.. logs];
        var planned = Math.Max(0d, scheduledMinutes - list.Sum(l => l.PlannedDowntimeMinutes));
        var unplanned = list.Sum(l => l.UnplannedDowntimeMinutes);
        var operating = Math.Max(0d, planned - unplanned);
        var good = list.Sum(l => l.GoodUnits);
        var scrap = list.Sum(l => l.ScrapUnits);
        var rework = list.Sum(l => l.ReworkUnits);
        var processed = good + scrap + rework;

        var availability = planned > 0d ? Clamp(operating / planned) : 0d;
        var performance = operating > 0d ? Clamp(processed * idealCycleMinutes / operating) : 0d;
        var quality = processed > 0d ? Clamp(good / processed) : 1d;

        return new OeeMetrics(
            Math.Round(scheduledMinutes, 3, MidpointRounding.AwayFromZero),
            Math.Round(planned, 3, MidpointRounding.AwayFromZero),
            Math.Round(unplanned, 3, MidpointRounding.AwayFromZero),
            Math.Round(operating, 3, MidpointRounding.AwayFromZero),
            Math.Round(good, 4, MidpointRounding.AwayFromZero),
            Math.Round(scrap, 4, MidpointRounding.AwayFromZero),
            Math.Round(rework, 4, MidpointRounding.AwayFromZero),
            Math.Round(availability, 6, MidpointRounding.AwayFromZero),
            Math.Round(performance, 6, MidpointRounding.AwayFromZero),
            Math.Round(quality, 6, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Gets the OEE factor (availability, performance or quality) a loss category belongs to.
    /// </summary>
    /// <param name="category">The loss category.</param>
    /// <returns>The factor name.</returns>
    public static string FactorOf(string category) => category switch
    {
        "setup_adjustment" or EquipmentFailure => "availability",
        "minor_stops" or "reduced_speed" => "performance",
        _ => "quality",
    };

    /// <summary>
    /// Distributes the lost minutes over the six big losses: unplanned downtime goes to the category of its log, scrap and rework
    /// are converted to minutes with the ideal cycle time (start-up rejects stay apart from process defects).
    /// </summary>
    /// <param name="logs">The OEE logs.</param>
    /// <param name="idealCycleMinutes">The ideal cycle time in minutes per unit.</param>
    /// <returns>The lost minutes by category, every category present.</returns>
    public static IReadOnlyDictionary<string, double> SixBigLosses(IEnumerable<OeeLogEntry> logs, double idealCycleMinutes)
    {
        ArgumentNullException.ThrowIfNull(logs);

        var losses = Categories.ToDictionary(c => c, _ => 0d, StringComparer.Ordinal);
        foreach (var log in logs)
        {
            var category = losses.ContainsKey(log.LossCategory) ? log.LossCategory : EquipmentFailure;
            losses[category] += log.UnplannedDowntimeMinutes;

            var defects = string.Equals(category, StartupRejects, StringComparison.Ordinal) ? StartupRejects : ProcessDefects;
            losses[defects] += (log.ScrapUnits + log.ReworkUnits) * idealCycleMinutes;
        }

        return losses.ToDictionary(p => p.Key, p => Math.Round(p.Value, 3, MidpointRounding.AwayFromZero), StringComparer.Ordinal);
    }

    /// <summary>
    /// Resolves the ideal cycle time of a work centre or machine: the machine value, else the average of the machines of the work
    /// centre, else the average run time per unit of its routing operations.
    /// </summary>
    /// <param name="machine">The machine, or null for the whole work centre.</param>
    /// <param name="workCenterMachines">The machines of the work centre.</param>
    /// <param name="operationRunMinutesPerUnit">The run time per unit of the operations on the work centre.</param>
    /// <returns>The ideal cycle time in minutes per unit (zero when unknown).</returns>
    public static double IdealCycleMinutes(MfgMachineCenter? machine, IEnumerable<MfgMachineCenter> workCenterMachines, IEnumerable<decimal> operationRunMinutesPerUnit)
    {
        ArgumentNullException.ThrowIfNull(workCenterMachines);
        ArgumentNullException.ThrowIfNull(operationRunMinutesPerUnit);

        if (machine?.IdealCycleTimeSecondsPerUnit > 0m)
        {
            return (double)machine.IdealCycleTimeSecondsPerUnit / 60d;
        }

        if (machine is null)
        {
            var cycles = workCenterMachines.Where(m => m.IdealCycleTimeSecondsPerUnit > 0m).Select(m => (double)m.IdealCycleTimeSecondsPerUnit).ToList();
            if (cycles.Count > 0)
            {
                return cycles.Average() / 60d;
            }
        }

        var runs = operationRunMinutesPerUnit.Where(r => r > 0m).Select(r => (double)r).ToList();
        return runs.Count > 0 ? runs.Average() : 0d;
    }

    /// <summary>
    /// Classifies the telemetry of a machine.
    /// </summary>
    /// <param name="lastSeenAt">When the machine last sent telemetry.</param>
    /// <param name="now">The current instant.</param>
    /// <returns>"never", "stale" or "online".</returns>
    public static string TelemetryStatus(DateTime? lastSeenAt, DateTime now)
    {
        return lastSeenAt switch
        {
            null => "never",
            { } seen when seen < now.AddMinutes(-StaleAfterMinutes) => "stale",
            _ => "online",
        };
    }

    private static double Clamp(double value) => Math.Max(0d, Math.Min(1d, value));
}
