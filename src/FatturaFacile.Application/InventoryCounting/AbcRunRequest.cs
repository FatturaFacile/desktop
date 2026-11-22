// <copyright file="AbcRunRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.InventoryCounting;

/// <summary>
/// Parameters of an ABC analysis run (null values use the policy defaults).
/// </summary>
public sealed record AbcRunRequest
{
    /// <summary>
    /// Gets or sets the analysis period in days.
    /// </summary>
    [JsonPropertyName("period_days")]
    public int? PeriodDays { get; set; }

    /// <summary>
    /// Gets or sets the cumulative percent (0..100) that closes class A.
    /// </summary>
    [JsonPropertyName("a_share")]
    public decimal? AShare { get; set; }

    /// <summary>
    /// Gets or sets the cumulative percent (0..100) that closes class B.
    /// </summary>
    [JsonPropertyName("b_share")]
    public decimal? BShare { get; set; }

    /// <summary>
    /// Gets or sets the warehouse to limit the run to.
    /// </summary>
    [JsonPropertyName("warehouse_id")]
    public string? WarehouseId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to apply the classes to the items.
    /// </summary>
    [JsonPropertyName("apply")]
    public bool? Apply { get; set; }
}
