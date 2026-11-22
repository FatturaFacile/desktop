// <copyright file="CapacityHeatmap.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The capacity saturation of the resources over a window.
/// </summary>
/// <param name="From">The from.</param>
/// <param name="To">The to.</param>
/// <param name="Granularity">The granularity.</param>
/// <param name="Thresholds">The thresholds.</param>
/// <param name="Periods">The first day of every period.</param>
/// <param name="Rows">One row per employee.</param>
public sealed record CapacityHeatmap(
    DateOnly From,
    DateOnly To,
    HeatmapGranularity Granularity,
    HeatmapThresholds Thresholds,
    IReadOnlyList<DateOnly> Periods,
    IReadOnlyList<HeatmapRow> Rows);
