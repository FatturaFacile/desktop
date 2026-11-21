// <copyright file="OeeLogEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Manufacturing;

/// <summary>
/// One OEE log row (downtime and production counts of a period).
/// </summary>
/// <param name="PlannedDowntimeMinutes">The planned downtime.</param>
/// <param name="UnplannedDowntimeMinutes">The unplanned downtime.</param>
/// <param name="GoodUnits">The good units.</param>
/// <param name="ScrapUnits">The scrapped units.</param>
/// <param name="ReworkUnits">The reworked units.</param>
/// <param name="LossCategory">The loss category (equipment_failure, setup_adjustment, minor_stops, reduced_speed, process_defects, startup_rejects).</param>
public sealed record OeeLogEntry(
    double PlannedDowntimeMinutes,
    double UnplannedDowntimeMinutes,
    double GoodUnits,
    double ScrapUnits,
    double ReworkUnits,
    string LossCategory);
