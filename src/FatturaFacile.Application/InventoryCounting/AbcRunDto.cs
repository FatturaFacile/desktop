// <copyright file="AbcRunDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.InventoryCounting;

/// <summary>
/// An ABC analysis run with its ranked items.
/// </summary>
public sealed record AbcRunDto
{
    /// <summary>
    /// Gets the run id.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the warehouse the run was limited to.
    /// </summary>
    [JsonPropertyName("warehouse_id")]
    public string? WarehouseId { get; init; }

    /// <summary>
    /// Gets the analysis period in days.
    /// </summary>
    [JsonPropertyName("period_days")]
    public int PeriodDays { get; init; }

    /// <summary>
    /// Gets the cumulative percent (0..100) that closes class A.
    /// </summary>
    [JsonPropertyName("a_share")]
    public decimal AShare { get; init; }

    /// <summary>
    /// Gets the cumulative percent (0..100) that closes class B.
    /// </summary>
    [JsonPropertyName("b_share")]
    public decimal BShare { get; init; }

    /// <summary>
    /// Gets the first day of the period.
    /// </summary>
    [JsonPropertyName("period_start")]
    public string? PeriodStart { get; init; }

    /// <summary>
    /// Gets the last day of the period.
    /// </summary>
    [JsonPropertyName("period_end")]
    public string? PeriodEnd { get; init; }

    /// <summary>
    /// Gets the total consumption value.
    /// </summary>
    [JsonPropertyName("total_consumption_value")]
    public decimal TotalConsumptionValue { get; init; }

    /// <summary>
    /// Gets the number of ranked items.
    /// </summary>
    [JsonPropertyName("item_count")]
    public int ItemCount { get; init; }

    /// <summary>
    /// Gets the number of class A items.
    /// </summary>
    [JsonPropertyName("class_a_count")]
    public int ClassACount { get; init; }

    /// <summary>
    /// Gets the number of class B items.
    /// </summary>
    [JsonPropertyName("class_b_count")]
    public int ClassBCount { get; init; }

    /// <summary>
    /// Gets the number of class C items.
    /// </summary>
    [JsonPropertyName("class_c_count")]
    public int ClassCCount { get; init; }

    /// <summary>
    /// Gets a value indicating whether the classes were applied to the items.
    /// </summary>
    [JsonPropertyName("applied")]
    public bool Applied { get; init; }

    /// <summary>
    /// Gets the ranked items.
    /// </summary>
    [JsonPropertyName("items")]
    public IReadOnlyList<AbcRunItemDto> Items { get; init; } = [];
}
