// <copyright file="HeatmapGranularity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The size of a heatmap cell.
/// </summary>
[JsonConverter(typeof(SnakeCaseEnumJsonConverter<HeatmapGranularity>))]
public enum HeatmapGranularity
{
    /// <summary>
    /// One week, starting on Monday.
    /// </summary>
    Week,

    /// <summary>
    /// One day.
    /// </summary>
    Day,
}
