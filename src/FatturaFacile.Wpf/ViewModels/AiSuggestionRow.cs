// <copyright file="AiSuggestionRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.AiCopilot;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Display row of a cached AI suggestion (localized labels, confidence band and offline queue state).
/// </summary>
internal sealed class AiSuggestionRow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AiSuggestionRow"/> class.
    /// </summary>
    /// <param name="suggestion">The cached suggestion.</param>
    /// <param name="localization">The localization service.</param>
    public AiSuggestionRow(AiSuggestion suggestion, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(suggestion);
        ArgumentNullException.ThrowIfNull(localization);

        Id = suggestion.Id;
        KindLabel = Translate(localization, "AiCopilot_Kind_" + suggestion.Kind, suggestion.Kind);
        Source = suggestion.SourceReference;
        Summary = suggestion.Reasoning ?? suggestion.SourceText ?? string.Empty;
        Band = AiConfidence.BandOf(suggestion.Confidence);
        BandLabel = localization["AiCopilot_Band_" + Band];
        ConfidenceText = suggestion.Confidence.ToString("P0", CultureInfo.CurrentCulture);
        AmountText = suggestion.Amount?.ToString("N2", CultureInfo.CurrentCulture) ?? string.Empty;
        PriorityLabel = Translate(localization, "AiCopilot_Priority_" + suggestion.Priority, suggestion.Priority);
        IsQueued = suggestion.QueuedDecision != AiQueuedDecision.None;
        StatusLabel = StatusOf(suggestion, localization);
        ErrorText = suggestion.LastError ?? string.Empty;
    }

    /// <summary>
    /// Gets the suggestion identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the localized suggestion kind.
    /// </summary>
    public string KindLabel { get; }

    /// <summary>
    /// Gets the source record reference.
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Gets the reasoning of the model (or the source text).
    /// </summary>
    public string Summary { get; }

    /// <summary>
    /// Gets the confidence band (high from 90%, medium from 70%, otherwise low).
    /// </summary>
    public AiConfidenceBand Band { get; }

    /// <summary>
    /// Gets the localized confidence band.
    /// </summary>
    public string BandLabel { get; }

    /// <summary>
    /// Gets the confidence as a percentage.
    /// </summary>
    public string ConfidenceText { get; }

    /// <summary>
    /// Gets the formatted amount, or an empty text.
    /// </summary>
    public string AmountText { get; }

    /// <summary>
    /// Gets the localized priority.
    /// </summary>
    public string PriorityLabel { get; }

    /// <summary>
    /// Gets a value indicating whether a decision is queued for this suggestion.
    /// </summary>
    public bool IsQueued { get; }

    /// <summary>
    /// Gets the localized status (pending, failed, or the queued decision).
    /// </summary>
    public string StatusLabel { get; }

    /// <summary>
    /// Gets the last server explanation or delivery error.
    /// </summary>
    public string ErrorText { get; }

    private static string Translate(ILocalizationService localization, string key, string fallback)
    {
        var text = localization[key];
        return string.Equals(text, key, StringComparison.Ordinal) ? fallback : text;
    }

    private static string StatusOf(AiSuggestion suggestion, ILocalizationService localization) => suggestion.QueuedDecision switch
    {
        AiQueuedDecision.Approve => localization["AiCopilot_StatusQueuedApprove"],
        AiQueuedDecision.Reject => localization["AiCopilot_StatusQueuedReject"],
        AiQueuedDecision.None => localization[string.Equals(suggestion.Status, "failed", StringComparison.Ordinal) ? "AiCopilot_StatusFailed" : "AiCopilot_StatusPending"],
        _ => string.Empty,
    };
}
