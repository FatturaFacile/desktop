// <copyright file="CountVarianceReportDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.InventoryCounting;

/// <summary>
/// The variance report of a count session.
/// </summary>
public sealed record CountVarianceReportDto
{
    /// <summary>
    /// Gets the session.
    /// </summary>
    [JsonPropertyName("session")]
    public CountSessionDto Session { get; init; } = new();

    /// <summary>
    /// Gets the totals.
    /// </summary>
    [JsonPropertyName("summary")]
    public CountVarianceSummaryDto Summary { get; init; } = new();

    /// <summary>
    /// Gets the lines.
    /// </summary>
    [JsonPropertyName("lines")]
    public IReadOnlyList<CountVarianceLineDto> Lines { get; init; } = [];

    /// <summary>
    /// Gets the approval levels.
    /// </summary>
    [JsonPropertyName("approvals")]
    public IReadOnlyList<CountApprovalDto> Approvals { get; init; } = [];

    /// <summary>
    /// Gets the adjustment, when created.
    /// </summary>
    [JsonPropertyName("adjustment")]
    public CountAdjustmentDto? Adjustment { get; init; }
}
