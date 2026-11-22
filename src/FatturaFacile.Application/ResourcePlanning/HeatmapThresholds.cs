// <copyright file="HeatmapThresholds.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The saturation thresholds of the heatmap in percent.
/// </summary>
/// <param name="Yellow">The load from which a cell is yellow.</param>
/// <param name="Red">The load above which a cell is red (overload).</param>
public sealed record HeatmapThresholds(
    double Yellow,
    double Red);
