// <copyright file="RebalanceStrategy.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// How an allocation is rebalanced.
/// </summary>
[JsonConverter(typeof(SnakeCaseEnumJsonConverter<RebalanceStrategy>))]
public enum RebalanceStrategy
{
    /// <summary>
    /// Moves the allocation keeping its duration.
    /// </summary>
    Shift = 0,

    /// <summary>
    /// Gives the allocation to another employee.
    /// </summary>
    Reassign = 1,

    /// <summary>
    /// Cuts the allocation in two parts.
    /// </summary>
    Split = 2,

    /// <summary>
    /// Changes the period or the hours per day.
    /// </summary>
    Resize = 3,
}
