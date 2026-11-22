// <copyright file="AnalyticalComparisonRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One center of the direct-versus-apportioned comparison, formatted for display.
/// </summary>
/// <param name="Label">The indented code and name.</param>
/// <param name="DirectText">The direct cost.</param>
/// <param name="InText">The apportioned inflow.</param>
/// <param name="OutText">The apportioned outflow.</param>
/// <param name="NetText">The net analytical cost.</param>
/// <param name="VarianceText">The variance against the direct cost.</param>
/// <param name="ShareText">The share of the total net cost.</param>
internal sealed record AnalyticalComparisonRow(string Label, string DirectText, string InText, string OutText, string NetText, string VarianceText, string ShareText);
