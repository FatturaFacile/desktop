// <copyright file="AiCopilotSnapshot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.AiCopilot;

/// <summary>
/// The locally cached state shown by the copilot panel.
/// </summary>
/// <param name="Suggestions">The open suggestions, most urgent first.</param>
/// <param name="Insights">The cached insights, or null before the first fetch.</param>
/// <param name="InsightsFetchedAt">When the insights were fetched (UTC).</param>
/// <param name="QueuedCount">The number of decisions waiting to be pushed.</param>
public sealed record AiCopilotSnapshot(
    IReadOnlyList<AiSuggestion> Suggestions,
    AiInsightsDto? Insights,
    DateTime? InsightsFetchedAt,
    int QueuedCount);
