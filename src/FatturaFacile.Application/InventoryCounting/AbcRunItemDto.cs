// <copyright file="AbcRunItemDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.InventoryCounting;

/// <summary>
/// One ranked stock item of an ABC analysis run.
/// </summary>
public sealed record AbcRunItemDto
{
    /// <summary>
    /// Gets the stock item id.
    /// </summary>
    [JsonPropertyName("stock_item_id")]
    public string StockItemId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the SKU.
    /// </summary>
    [JsonPropertyName("sku")]
    public string Sku { get; init; } = string.Empty;

    /// <summary>
    /// Gets the item name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the rank by consumption value (1 is highest).
    /// </summary>
    [JsonPropertyName("rank")]
    public int Rank { get; init; }

    /// <summary>
    /// Gets the consumption value over the period.
    /// </summary>
    [JsonPropertyName("consumption_value")]
    public decimal ConsumptionValue { get; init; }

    /// <summary>
    /// Gets the cumulative share of the total consumption value, in percent (0..100).
    /// </summary>
    [JsonPropertyName("cumulative_share")]
    public decimal CumulativeShare { get; init; }

    /// <summary>
    /// Gets the class (A, B or C).
    /// </summary>
    [JsonPropertyName("abc_class")]
    public string AbcClass { get; init; } = string.Empty;

    /// <summary>
    /// Gets the class before this run.
    /// </summary>
    [JsonPropertyName("previous_class")]
    public string? PreviousClass { get; init; }

    /// <summary>
    /// Gets a value indicating whether a manual override fixes the class.
    /// </summary>
    [JsonPropertyName("is_manual_override")]
    public bool IsManualOverride { get; init; }

    /// <summary>
    /// Gets a value indicating whether the item had no consumption history.
    /// </summary>
    [JsonPropertyName("is_new_item")]
    public bool IsNewItem { get; init; }
}
