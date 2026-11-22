// <copyright file="CountVarianceLineDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.InventoryCounting;

/// <summary>
/// One count line with system, counted and final quantities and its variance.
/// </summary>
public sealed record CountVarianceLineDto
{
    /// <summary>
    /// Gets the line id.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the bin id.
    /// </summary>
    [JsonPropertyName("warehouse_bin_id")]
    public string? WarehouseBinId { get; init; }

    /// <summary>
    /// Gets the bin barcode.
    /// </summary>
    [JsonPropertyName("bin_barcode")]
    public string? BinBarcode { get; init; }

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
    [JsonPropertyName("item_name")]
    public string ItemName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the item barcode.
    /// </summary>
    [JsonPropertyName("barcode")]
    public string? Barcode { get; init; }

    /// <summary>
    /// Gets the lot id.
    /// </summary>
    [JsonPropertyName("lot_id")]
    public string? LotId { get; init; }

    /// <summary>
    /// Gets the last counted quantity, or null when not counted.
    /// </summary>
    [JsonPropertyName("counted_quantity")]
    public decimal? CountedQuantity { get; init; }

    /// <summary>
    /// Gets a value indicating whether the line was counted.
    /// </summary>
    [JsonPropertyName("is_counted")]
    public bool IsCounted { get; init; }

    /// <summary>
    /// Gets a value indicating whether a recount is pending.
    /// </summary>
    [JsonPropertyName("recount_required")]
    public bool RecountRequired { get; init; }

    /// <summary>
    /// Gets the recount round.
    /// </summary>
    [JsonPropertyName("recount_round")]
    public int RecountRound { get; init; }

    /// <summary>
    /// Gets the why a recount was requested.
    /// </summary>
    [JsonPropertyName("recount_reason")]
    public string? RecountReason { get; init; }

    /// <summary>
    /// Gets the free notes.
    /// </summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }

    /// <summary>
    /// Gets the when the line was counted.
    /// </summary>
    [JsonPropertyName("counted_at")]
    public string? CountedAt { get; init; }

    /// <summary>
    /// Gets the system quantity at snapshot.
    /// </summary>
    [JsonPropertyName("system_quantity")]
    public decimal SystemQuantity { get; init; }

    /// <summary>
    /// Gets the unit cost.
    /// </summary>
    [JsonPropertyName("unit_cost")]
    public decimal UnitCost { get; init; }

    /// <summary>
    /// Gets the system value.
    /// </summary>
    [JsonPropertyName("system_value")]
    public decimal SystemValue { get; init; }

    /// <summary>
    /// Gets the first counted quantity.
    /// </summary>
    [JsonPropertyName("first_counted_quantity")]
    public decimal? FirstCountedQuantity { get; init; }

    /// <summary>
    /// Gets the final quantity.
    /// </summary>
    [JsonPropertyName("final_quantity")]
    public decimal? FinalQuantity { get; init; }

    /// <summary>
    /// Gets the variance quantity (final minus system).
    /// </summary>
    [JsonPropertyName("variance_quantity")]
    public decimal VarianceQuantity { get; init; }

    /// <summary>
    /// Gets the variance value.
    /// </summary>
    [JsonPropertyName("variance_value")]
    public decimal VarianceValue { get; init; }

    /// <summary>
    /// Gets the variance percentage, null when undefined.
    /// </summary>
    [JsonPropertyName("variance_percentage")]
    public decimal? VariancePercentage { get; init; }

    /// <summary>
    /// Gets a value indicating whether the percentage is undefined (system quantity is zero).
    /// </summary>
    [JsonPropertyName("variance_percentage_undefined")]
    public bool VariancePercentageUndefined { get; init; }
}
