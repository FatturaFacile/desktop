// <copyright file="CreateCountSessionRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.InventoryCounting;

/// <summary>
/// Parameters of a new count session.
/// </summary>
public sealed record CreateCountSessionRequest
{
    /// <summary>
    /// Gets or sets the warehouse id.
    /// </summary>
    [JsonPropertyName("warehouse_id")]
    public string WarehouseId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the count type.
    /// </summary>
    [JsonPropertyName("count_type")]
    public string CountType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scheduled date (yyyy-MM-dd).
    /// </summary>
    [JsonPropertyName("scheduled_date")]
    public string ScheduledDate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to freeze the stock.
    /// </summary>
    [JsonPropertyName("freezes_stock")]
    public bool? FreezesStock { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the count is blind.
    /// </summary>
    [JsonPropertyName("is_blind")]
    public bool? IsBlind { get; set; }

    /// <summary>
    /// Gets or sets the assigned counter.
    /// </summary>
    [JsonPropertyName("assigned_to")]
    public string? AssignedTo { get; set; }

    /// <summary>
    /// Gets or sets the zone to count.
    /// </summary>
    [JsonPropertyName("warehouse_zone_id")]
    public string? WarehouseZoneId { get; set; }

    /// <summary>
    /// Gets or sets the bins to count.
    /// </summary>
    [JsonPropertyName("bin_ids")]
    public IReadOnlyList<string>? BinIds { get; set; }

    /// <summary>
    /// Gets or sets the free notes.
    /// </summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}
