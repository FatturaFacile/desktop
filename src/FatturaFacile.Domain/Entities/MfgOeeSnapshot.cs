// <copyright file="MfgOeeSnapshot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// The OEE summary of a work centre (or machine) over a period, cached from the web-api so it can be read offline.
/// </summary>
public class MfgOeeSnapshot : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the server identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the local creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the local update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the work centre identifier.
    /// </summary>
    public Guid WorkCenterId { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier.
    /// </summary>
    public Guid? MachineCenterId { get; set; }

    /// <summary>
    /// Gets or sets the start of the period.
    /// </summary>
    public DateTime FromUtc { get; set; }

    /// <summary>
    /// Gets or sets the end of the period.
    /// </summary>
    public DateTime ToUtc { get; set; }

    /// <summary>
    /// Gets or sets the scheduled minutes.
    /// </summary>
    public double ScheduledMinutes { get; set; }

    /// <summary>
    /// Gets or sets the planned production minutes.
    /// </summary>
    public double PlannedProductionMinutes { get; set; }

    /// <summary>
    /// Gets or sets the unplanned downtime minutes.
    /// </summary>
    public double UnplannedDowntimeMinutes { get; set; }

    /// <summary>
    /// Gets or sets the operating minutes.
    /// </summary>
    public double OperatingMinutes { get; set; }

    /// <summary>
    /// Gets or sets the good units.
    /// </summary>
    public double GoodUnits { get; set; }

    /// <summary>
    /// Gets or sets the scrapped units.
    /// </summary>
    public double ScrapUnits { get; set; }

    /// <summary>
    /// Gets or sets the reworked units.
    /// </summary>
    public double ReworkUnits { get; set; }

    /// <summary>
    /// Gets or sets the availability percentage.
    /// </summary>
    public double AvailabilityPercent { get; set; }

    /// <summary>
    /// Gets or sets the performance percentage.
    /// </summary>
    public double PerformancePercent { get; set; }

    /// <summary>
    /// Gets or sets the quality percentage.
    /// </summary>
    public double QualityPercent { get; set; }

    /// <summary>
    /// Gets or sets the OEE percentage.
    /// </summary>
    public double OeePercent { get; set; }

    /// <summary>
    /// Gets or sets the OEE target percentage.
    /// </summary>
    public double OeeTargetPercent { get; set; }

    /// <summary>
    /// Gets or sets the gap between the OEE and its target in percentage points.
    /// </summary>
    public double GapToTargetPercent { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the OEE reaches the target.
    /// </summary>
    public bool MeetsTarget { get; set; }

    /// <summary>
    /// Gets or sets the six big losses payload.
    /// </summary>
    public string SixBigLossesJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the machine telemetry payload.
    /// </summary>
    public string MachinesJson { get; set; } = "[]";

    /// <summary>
    /// Gets or sets when the snapshot was pulled from the server.
    /// </summary>
    public DateTime SyncedAtUtc { get; set; }
}
