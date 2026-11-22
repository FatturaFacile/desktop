// <copyright file="CountAdjustmentDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.InventoryCounting;

/// <summary>
/// The inventory adjustment created by a reconciliation.
/// </summary>
public sealed record CountAdjustmentDto
{
    /// <summary>
    /// Gets the adjustment id.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the adjustment number.
    /// </summary>
    [JsonPropertyName("adjustment_number")]
    public string? AdjustmentNumber { get; init; }

    /// <summary>
    /// Gets the status.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    /// <summary>
    /// Gets the net valuation change.
    /// </summary>
    [JsonPropertyName("net_valuation_change")]
    public decimal NetValuationChange { get; init; }

    /// <summary>
    /// Gets the general ledger journal entry.
    /// </summary>
    [JsonPropertyName("journal_entry_id")]
    public string? JournalEntryId { get; init; }

    /// <summary>
    /// Gets the when the journal entry was posted.
    /// </summary>
    [JsonPropertyName("gl_posted_at")]
    public string? GlPostedAt { get; init; }
}
