// <copyright file="AiSuggestionDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AiCopilot;

/// <summary>
/// A suggestion of the web-api AI queue (JSON structures are kept as raw JSON text).
/// </summary>
/// <param name="Id">The suggestion identifier.</param>
/// <param name="Kind">The suggestion kind.</param>
/// <param name="Workflow">The producing workflow.</param>
/// <param name="SourceType">The source record type.</param>
/// <param name="SourceReference">The source record reference.</param>
/// <param name="SourceText">The source text.</param>
/// <param name="ProposedPayloadJson">The proposed payload as a JSON object.</param>
/// <param name="FieldConfidencesJson">The per-field confidences as a JSON object.</param>
/// <param name="Confidence">The overall confidence between 0 and 1.</param>
/// <param name="Amount">The amount involved.</param>
/// <param name="Reasoning">The model reasoning.</param>
/// <param name="Status">The server status.</param>
/// <param name="Priority">The priority (low, normal, high).</param>
/// <param name="ProviderUsed">The AI provider.</param>
/// <param name="ModelUsed">The AI model.</param>
/// <param name="ExpiresAt">The expiry instant in UTC.</param>
/// <param name="CreatedAt">The creation instant in UTC.</param>
public sealed record AiSuggestionDto(
    Guid Id,
    string Kind,
    string Workflow,
    string SourceType,
    string SourceReference,
    string? SourceText,
    string ProposedPayloadJson,
    string FieldConfidencesJson,
    double Confidence,
    decimal? Amount,
    string? Reasoning,
    string Status,
    string Priority,
    string? ProviderUsed,
    string? ModelUsed,
    DateTime? ExpiresAt,
    DateTime? CreatedAt);
