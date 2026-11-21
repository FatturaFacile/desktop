// <copyright file="MfgMachineCenter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A machine of a work centre mirrored from the web-api (Mod 061).
/// </summary>
public class MfgMachineCenter : ITenantScopedEntity
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
    /// Gets or sets the machine code.
    /// </summary>
    public string MachineCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the work centre identifier.
    /// </summary>
    public Guid WorkCenterId { get; set; }

    /// <summary>
    /// Gets or sets the fixed asset identifier.
    /// </summary>
    public Guid? FixedAssetId { get; set; }

    /// <summary>
    /// Gets or sets the manufacturer.
    /// </summary>
    public string? Manufacturer { get; set; }

    /// <summary>
    /// Gets or sets the model.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Gets or sets the serial number.
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the machine is Industria 4.0 enabled.
    /// </summary>
    public bool IsIndustria40Enabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the machine is Transizione 5.0 enabled.
    /// </summary>
    public bool IsTransizione50Enabled { get; set; }

    /// <summary>
    /// Gets or sets the telemetry status.
    /// </summary>
    public string TelemetryStatus { get; set; } = "never";

    /// <summary>
    /// Gets or sets when telemetry was last received.
    /// </summary>
    public DateTime? TelemetryLastSeenAt { get; set; }

    /// <summary>
    /// Gets or sets the ideal cycle time in seconds per unit.
    /// </summary>
    public decimal IdealCycleTimeSecondsPerUnit { get; set; }

    /// <summary>
    /// Gets or sets the number of parallel production units.
    /// </summary>
    public int ParallelCapacityUnits { get; set; } = 1;

    /// <summary>
    /// Gets or sets the efficiency percentage overriding the work centre one.
    /// </summary>
    public decimal? EfficiencyFactorPercent { get; set; }

    /// <summary>
    /// Gets or sets the machine run cost per hour overriding the work centre one.
    /// </summary>
    public decimal? HourlyMachineRunCostRateOverride { get; set; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; set; } = "active";

    /// <summary>
    /// Gets or sets when the row was pulled from the server.
    /// </summary>
    public DateTime SyncedAtUtc { get; set; }
}
