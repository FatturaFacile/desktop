// <copyright file="DssWaterfallBar.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One category of the variance waterfall, split into the stacked-column parts the chart draws. A change floats on an invisible
/// base; a bar that crosses the zero line is split into its positive and negative part, as the chart stacks the two signs apart.
/// </summary>
/// <param name="Label">The category label.</param>
/// <param name="Base">The invisible base the bar floats on.</param>
/// <param name="TotalPositive">The positive part of a total (baseline or actual).</param>
/// <param name="TotalNegative">The negative part of a total.</param>
/// <param name="IncreasePositive">The positive part of an increase.</param>
/// <param name="IncreaseNegative">The negative part of an increase.</param>
/// <param name="DecreasePositive">The positive part of a decrease.</param>
/// <param name="DecreaseNegative">The negative part of a decrease.</param>
internal sealed record DssWaterfallBar(
    string Label,
    double Base,
    double TotalPositive,
    double TotalNegative,
    double IncreasePositive,
    double IncreaseNegative,
    double DecreasePositive,
    double DecreaseNegative);
