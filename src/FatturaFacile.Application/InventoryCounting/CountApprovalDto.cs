// <copyright file="CountApprovalDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.InventoryCounting;

/// <summary>
/// One approval level of a count reconciliation.
/// </summary>
public sealed record CountApprovalDto
{
    /// <summary>
    /// Gets the approval id.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Gets the level (1 or 2).
    /// </summary>
    [JsonPropertyName("level")]
    public int Level { get; init; }

    /// <summary>
    /// Gets the role that must decide.
    /// </summary>
    [JsonPropertyName("required_role")]
    public string? RequiredRole { get; init; }

    /// <summary>
    /// Gets the status (pending, approved, rejected or cancelled).
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Gets the who decided.
    /// </summary>
    [JsonPropertyName("decided_by")]
    public string? DecidedBy { get; init; }

    /// <summary>
    /// Gets the when it was decided.
    /// </summary>
    [JsonPropertyName("decided_at")]
    public string? DecidedAt { get; init; }

    /// <summary>
    /// Gets the decision notes.
    /// </summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}
