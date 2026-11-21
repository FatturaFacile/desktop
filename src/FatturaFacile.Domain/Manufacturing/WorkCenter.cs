// <copyright file="WorkCenter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Manufacturing;

/// <summary>
/// A work centre with its finite capacity and cost rates.
/// </summary>
public class WorkCenter : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the local identifier (equals the server identifier for pulled records).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the local tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the work centre code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the nominal capacity in hours per day.
    /// </summary>
    public decimal CapacityHoursPerDay { get; set; } = 8m;

    /// <summary>
    /// Gets or sets the efficiency factor (1 = 100%).
    /// </summary>
    public decimal EfficiencyRate { get; set; } = 1m;

    /// <summary>
    /// Gets or sets the availability factor (1 = 100%).
    /// </summary>
    public decimal AvailabilityRate { get; set; } = 1m;

    /// <summary>
    /// Gets or sets the labour cost rate per hour.
    /// </summary>
    public decimal HourlyCostRate { get; set; }

    /// <summary>
    /// Gets or sets the overhead absorption rate per machine hour.
    /// </summary>
    public decimal HourlyOverheadRate { get; set; }

    /// <summary>
    /// Gets or sets the server status.
    /// </summary>
    public string Status { get; set; } = "active";

    /// <summary>
    /// Gets or sets the creation instant (UTC).
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last local modification instant (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the server updated_at stamp seen at the last pull.
    /// </summary>
    public string? ServerVersion { get; set; }
}
