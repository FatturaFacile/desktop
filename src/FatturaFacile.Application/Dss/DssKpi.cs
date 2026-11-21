// <copyright file="DssKpi.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Dss;

/// <summary>
/// One Balanced Scorecard indicator.
/// </summary>
/// <param name="Key">The indicator key (for example "revenue").</param>
/// <param name="Group">The scorecard group: financial, customer or efficiency.</param>
/// <param name="Unit">The unit: currency, percent, days or ratio.</param>
/// <param name="Available">False while the indicator has no data source.</param>
/// <param name="Value">The value.</param>
/// <param name="ComparisonValue">The value of the comparison baseline.</param>
/// <param name="Delta">The change against the baseline.</param>
/// <param name="DeltaPct">The change in percent.</param>
/// <param name="Favorable">Whether the change is an improvement.</param>
/// <param name="Sparkline">The monthly trend.</param>
/// <param name="Target">The target, when the indicator has one.</param>
public sealed record DssKpi(
    string Key,
    string Group,
    string Unit,
    bool Available,
    double? Value,
    double? ComparisonValue,
    double? Delta,
    double? DeltaPct,
    bool? Favorable,
    IReadOnlyList<double> Sparkline,
    double? Target);
