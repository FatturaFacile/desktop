// <copyright file="CountVarianceSummaryDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.InventoryCounting;

/// <summary>
/// Totals of a variance report.
/// </summary>
public sealed record CountVarianceSummaryDto
{
    /// <summary>
    /// Gets the number of lines.
    /// </summary>
    [JsonPropertyName("lines")]
    public int Lines { get; init; }

    /// <summary>
    /// Gets the number of counted lines.
    /// </summary>
    [JsonPropertyName("counted_lines")]
    public int CountedLines { get; init; }

    /// <summary>
    /// Gets the number of uncounted lines.
    /// </summary>
    [JsonPropertyName("uncounted_lines")]
    public int UncountedLines { get; init; }

    /// <summary>
    /// Gets the number of lines with a variance.
    /// </summary>
    [JsonPropertyName("differing_lines")]
    public int DifferingLines { get; init; }

    /// <summary>
    /// Gets the number of lines waiting for a recount.
    /// </summary>
    [JsonPropertyName("recount_pending_lines")]
    public int RecountPendingLines { get; init; }

    /// <summary>
    /// Gets the total system value.
    /// </summary>
    [JsonPropertyName("system_value_total")]
    public decimal SystemValueTotal { get; init; }

    /// <summary>
    /// Gets the gross variance value (sum of absolute variances).
    /// </summary>
    [JsonPropertyName("gross_variance_value")]
    public decimal GrossVarianceValue { get; init; }

    /// <summary>
    /// Gets the net variance value.
    /// </summary>
    [JsonPropertyName("net_variance_value")]
    public decimal NetVarianceValue { get; init; }

    /// <summary>
    /// Gets the approval mode.
    /// </summary>
    [JsonPropertyName("approval_mode")]
    public string? ApprovalMode { get; init; }

    /// <summary>
    /// Gets the approval level the variance requires, or null.
    /// </summary>
    [JsonPropertyName("suggested_approval_level")]
    public int? SuggestedApprovalLevel { get; init; }

    /// <summary>
    /// Gets a value indicating whether the variance is within the auto-approve tolerance.
    /// </summary>
    [JsonPropertyName("within_tolerance")]
    public bool WithinTolerance { get; init; }
}
