// <copyright file="CapacityBand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The saturation band of a capacity cell.
/// </summary>
[JsonConverter(typeof(SnakeCaseEnumJsonConverter<CapacityBand>))]
public enum CapacityBand
{
    /// <summary>
    /// No net capacity and nothing booked.
    /// </summary>
    None = 0,

    /// <summary>
    /// Below 80 percent.
    /// </summary>
    Green = 1,

    /// <summary>
    /// From 80 to 100 percent.
    /// </summary>
    Yellow = 2,

    /// <summary>
    /// Above 100 percent: overload.
    /// </summary>
    Red = 3,
}
