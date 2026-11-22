// <copyright file="CycleCountPlanDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.InventoryCounting;

/// <summary>
/// A cycle count plan.
/// </summary>
public sealed record CycleCountPlanDto
{
    /// <summary>
    /// Gets the plan id.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the warehouse id.
    /// </summary>
    [JsonPropertyName("warehouse_id")]
    public string WarehouseId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the zone id.
    /// </summary>
    [JsonPropertyName("warehouse_zone_id")]
    public string? WarehouseZoneId { get; init; }

    /// <summary>
    /// Gets the plan name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the ABC class counted by the plan.
    /// </summary>
    [JsonPropertyName("abc_class")]
    public string? AbcClass { get; init; }

    /// <summary>
    /// Gets the cadence name.
    /// </summary>
    [JsonPropertyName("cadence")]
    public string? Cadence { get; init; }

    /// <summary>
    /// Gets the interval in days.
    /// </summary>
    [JsonPropertyName("interval_days")]
    public int IntervalDays { get; init; }

    /// <summary>
    /// Gets a value indicating whether the counts freeze the stock.
    /// </summary>
    [JsonPropertyName("freezes_stock")]
    public bool FreezesStock { get; init; }

    /// <summary>
    /// Gets a value indicating whether the plan is active.
    /// </summary>
    [JsonPropertyName("is_active")]
    public bool IsActive { get; init; }

    /// <summary>
    /// Gets the when the plan last ran.
    /// </summary>
    [JsonPropertyName("last_run_at")]
    public string? LastRunAt { get; init; }

    /// <summary>
    /// Gets the next due date.
    /// </summary>
    [JsonPropertyName("next_run_on")]
    public string? NextRunOn { get; init; }
}
