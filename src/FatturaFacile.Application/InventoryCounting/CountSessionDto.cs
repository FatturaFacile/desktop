// <copyright file="CountSessionDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.InventoryCounting;

/// <summary>
/// A physical inventory count session.
/// </summary>
public sealed record CountSessionDto
{
    /// <summary>
    /// Gets the session id.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the session number.
    /// </summary>
    [JsonPropertyName("session_number")]
    public string SessionNumber { get; init; } = string.Empty;

    /// <summary>
    /// Gets the warehouse id.
    /// </summary>
    [JsonPropertyName("warehouse_id")]
    public string? WarehouseId { get; init; }

    /// <summary>
    /// Gets the zone id.
    /// </summary>
    [JsonPropertyName("warehouse_zone_id")]
    public string? WarehouseZoneId { get; init; }

    /// <summary>
    /// Gets the count type (full_physical, cycle_count_abc, spot_check or zone_sweep).
    /// </summary>
    [JsonPropertyName("count_type")]
    public string CountType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the status (in_progress, pending_approval, reconciled or cancelled).
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Gets the scheduled date.
    /// </summary>
    [JsonPropertyName("scheduled_date")]
    public string? ScheduledDate { get; init; }

    /// <summary>
    /// Gets the when the system quantities were snapshotted.
    /// </summary>
    [JsonPropertyName("snapshot_at")]
    public string? SnapshotAt { get; init; }

    /// <summary>
    /// Gets a value indicating whether counters do not see the system quantity.
    /// </summary>
    [JsonPropertyName("is_blind")]
    public bool IsBlind { get; init; }

    /// <summary>
    /// Gets a value indicating whether the count freezes the stock.
    /// </summary>
    [JsonPropertyName("freezes_stock")]
    public bool FreezesStock { get; init; }

    /// <summary>
    /// Gets the freeze scope.
    /// </summary>
    [JsonPropertyName("freeze_scope")]
    public string? FreezeScope { get; init; }

    /// <summary>
    /// Gets the when the stock was frozen, or null when not frozen.
    /// </summary>
    [JsonPropertyName("frozen_at")]
    public string? FrozenAt { get; init; }

    /// <summary>
    /// Gets the assigned counter.
    /// </summary>
    [JsonPropertyName("assigned_to")]
    public string? AssignedTo { get; init; }

    /// <summary>
    /// Gets the adjustment created by the reconciliation.
    /// </summary>
    [JsonPropertyName("inventory_adjustment_id")]
    public string? InventoryAdjustmentId { get; init; }

    /// <summary>
    /// Gets the approval mode (workflow or multi_level).
    /// </summary>
    [JsonPropertyName("approval_mode")]
    public string? ApprovalMode { get; init; }

    /// <summary>
    /// Gets the number of approval levels required.
    /// </summary>
    [JsonPropertyName("approval_level_required")]
    public int? ApprovalLevelRequired { get; init; }

    /// <summary>
    /// Gets the final approver.
    /// </summary>
    [JsonPropertyName("approved_by")]
    public string? ApprovedBy { get; init; }

    /// <summary>
    /// Gets the when the count was approved.
    /// </summary>
    [JsonPropertyName("approved_at")]
    public string? ApprovedAt { get; init; }
}
