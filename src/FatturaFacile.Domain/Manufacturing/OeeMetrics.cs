// <copyright file="OeeMetrics.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Manufacturing;

/// <summary>
/// The OEE figures of a period: availability, performance and quality as ratios (0..1) and as percentages.
/// </summary>
/// <param name="ScheduledMinutes">The scheduled minutes of the calendar.</param>
/// <param name="PlannedProductionMinutes">The scheduled minutes minus the planned downtime.</param>
/// <param name="UnplannedDowntimeMinutes">The unplanned downtime.</param>
/// <param name="OperatingMinutes">The planned production minutes minus the unplanned downtime.</param>
/// <param name="GoodUnits">The good units.</param>
/// <param name="ScrapUnits">The scrapped units.</param>
/// <param name="ReworkUnits">The reworked units.</param>
/// <param name="Availability">The availability ratio.</param>
/// <param name="Performance">The performance ratio.</param>
/// <param name="Quality">The quality ratio.</param>
public sealed record OeeMetrics(
    double ScheduledMinutes,
    double PlannedProductionMinutes,
    double UnplannedDowntimeMinutes,
    double OperatingMinutes,
    double GoodUnits,
    double ScrapUnits,
    double ReworkUnits,
    double Availability,
    double Performance,
    double Quality)
{
    /// <summary>
    /// Gets the OEE ratio (availability x performance x quality).
    /// </summary>
    public double Oee => Availability * Performance * Quality;

    /// <summary>
    /// Gets the availability percentage.
    /// </summary>
    public double AvailabilityPercent => Math.Round(Availability * 100d, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Gets the performance percentage.
    /// </summary>
    public double PerformancePercent => Math.Round(Performance * 100d, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Gets the quality percentage.
    /// </summary>
    public double QualityPercent => Math.Round(Quality * 100d, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Gets the OEE percentage.
    /// </summary>
    public double OeePercent => Math.Round(Oee * 100d, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Gets the total processed units (good, scrap and rework).
    /// </summary>
    public double TotalUnits => GoodUnits + ScrapUnits + ReworkUnits;
}
