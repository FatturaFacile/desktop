// <copyright file="DssKpiCard.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A Balanced Scorecard card: the indicator, its change against the baseline, the trend and the target gauge.
/// </summary>
/// <param name="Key">The indicator key.</param>
/// <param name="Title">The localized title.</param>
/// <param name="ValueText">The formatted value, or a dash.</param>
/// <param name="DeltaText">The formatted change, or an empty string without a comparison.</param>
/// <param name="DeltaState">Good, Bad or Neutral, for the colour of the change.</param>
/// <param name="IsAvailable">False while the indicator has no data source.</param>
/// <param name="Sparkline">The monthly trend.</param>
/// <param name="TargetProgress">The share of the target reached (0..100), or null without a target.</param>
internal sealed record DssKpiCard(
    string Key,
    string Title,
    string ValueText,
    string DeltaText,
    string DeltaState,
    bool IsAvailable,
    IReadOnlyList<DssSparkPoint> Sparkline,
    double? TargetProgress)
{
    /// <summary>
    /// Gets a value indicating whether the card has a trend to draw.
    /// </summary>
    public bool HasSparkline => Sparkline.Count > 1;

    /// <summary>
    /// Gets a value indicating whether the card has a target gauge.
    /// </summary>
    public bool HasTarget => TargetProgress is not null;

    /// <summary>
    /// Gets a value indicating whether the card shows a change.
    /// </summary>
    public bool HasDelta => DeltaText.Length > 0;
}
