// <copyright file="UtilizationStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// Where the actual billable utilization stands against the target.
/// </summary>
[JsonConverter(typeof(SnakeCaseEnumJsonConverter<UtilizationStatus>))]
public enum UtilizationStatus
{
    /// <summary>
    /// Within 5 points of the target.
    /// </summary>
    OnTarget,

    /// <summary>
    /// More than 5 points below the target.
    /// </summary>
    BelowTarget,

    /// <summary>
    /// More than 5 points above the target.
    /// </summary>
    AboveTarget,
}
