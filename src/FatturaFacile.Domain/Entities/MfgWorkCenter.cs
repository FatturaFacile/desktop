// <copyright file="MfgWorkCenter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A shop-floor work centre mirrored from the web-api (Mod 061); a read-only offline cache.
/// </summary>
public class MfgWorkCenter : ITenantScopedEntity
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
    /// Gets or sets the work centre code.
    /// </summary>
    public string WorkCenterCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the department code.
    /// </summary>
    public string? DepartmentCode { get; set; }

    /// <summary>
    /// Gets or sets the daily capacity in hours.
    /// </summary>
    public decimal CapacityHoursPerDay { get; set; }

    /// <summary>
    /// Gets or sets the efficiency percentage (100 = nominal).
    /// </summary>
    public decimal EfficiencyRate { get; set; } = 100m;

    /// <summary>
    /// Gets or sets the availability percentage.
    /// </summary>
    public decimal AvailabilityRate { get; set; } = 100m;

    /// <summary>
    /// Gets or sets the direct labour cost per hour.
    /// </summary>
    public decimal HourlyCostRate { get; set; }

    /// <summary>
    /// Gets or sets the overhead cost per hour.
    /// </summary>
    public decimal HourlyOverheadRate { get; set; }

    /// <summary>
    /// Gets or sets the setup cost per hour.
    /// </summary>
    public decimal HourlySetupCostRate { get; set; }

    /// <summary>
    /// Gets or sets the machine run cost per hour.
    /// </summary>
    public decimal HourlyMachineRunCostRate { get; set; }

    /// <summary>
    /// Gets or sets the ISO currency code of the rates.
    /// </summary>
    public string CurrencyCode { get; set; } = "CHF";

    /// <summary>
    /// Gets or sets the status (active, maintenance, inactive, decommissioned).
    /// </summary>
    public string Status { get; set; } = "active";

    /// <summary>
    /// Gets or sets the working calendar identifier.
    /// </summary>
    public Guid? WorkingCalendarId { get; set; }

    /// <summary>
    /// Gets or sets the parent work centre identifier.
    /// </summary>
    public Guid? ParentWorkCenterId { get; set; }

    /// <summary>
    /// Gets or sets the cost centre identifier.
    /// </summary>
    public Guid? CostCenterId { get; set; }

    /// <summary>
    /// Gets or sets the warehouse identifier.
    /// </summary>
    public Guid? WarehouseId { get; set; }

    /// <summary>
    /// Gets or sets the number of parallel production units.
    /// </summary>
    public int ParallelCapacityUnits { get; set; } = 1;

    /// <summary>
    /// Gets or sets the OEE target percentage.
    /// </summary>
    public decimal OeeTargetPercent { get; set; } = 85m;

    /// <summary>
    /// Gets or sets the availability target percentage.
    /// </summary>
    public decimal? AvailabilityTargetPercent { get; set; }

    /// <summary>
    /// Gets or sets the performance target percentage.
    /// </summary>
    public decimal? PerformanceTargetPercent { get; set; }

    /// <summary>
    /// Gets or sets the quality target percentage.
    /// </summary>
    public decimal? QualityTargetPercent { get; set; }

    /// <summary>
    /// Gets or sets the default queue time before an operation in minutes.
    /// </summary>
    public decimal DefaultQueueWaitMinutes { get; set; }

    /// <summary>
    /// Gets or sets the default move time after an operation in minutes.
    /// </summary>
    public decimal DefaultMoveTimeMinutes { get; set; }

    /// <summary>
    /// Gets or sets the alternate work centre identifiers as a JSON array.
    /// </summary>
    public string AlternateWorkCenterIdsJson { get; set; } = "[]";

    /// <summary>
    /// Gets or sets the legacy weekly available hours as a JSON array of weekday and hours.
    /// </summary>
    public string WeeklyHoursJson { get; set; } = "[]";

    /// <summary>
    /// Gets or sets when the row was pulled from the server.
    /// </summary>
    public DateTime SyncedAtUtc { get; set; }
}
