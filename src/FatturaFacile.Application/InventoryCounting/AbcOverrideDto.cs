// <copyright file="AbcOverrideDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.InventoryCounting;

/// <summary>
/// A manual ABC class override.
/// </summary>
public sealed record AbcOverrideDto
{
    /// <summary>
    /// Gets the override id.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Gets the stock item id.
    /// </summary>
    [JsonPropertyName("stock_item_id")]
    public string StockItemId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the SKU.
    /// </summary>
    [JsonPropertyName("sku")]
    public string? Sku { get; init; }

    /// <summary>
    /// Gets the item name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Gets the forced class, or null when the override clears the class.
    /// </summary>
    [JsonPropertyName("abc_class")]
    public string? AbcClass { get; init; }

    /// <summary>
    /// Gets the justification.
    /// </summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; init; }
}
