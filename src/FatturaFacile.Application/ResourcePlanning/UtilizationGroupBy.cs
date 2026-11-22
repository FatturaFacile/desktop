// <copyright file="UtilizationGroupBy.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// What the utilization report is grouped by.
/// </summary>
[JsonConverter(typeof(SnakeCaseEnumJsonConverter<UtilizationGroupBy>))]
public enum UtilizationGroupBy
{
    /// <summary>
    /// One row per employee.
    /// </summary>
    Resource,

    /// <summary>
    /// One row per department.
    /// </summary>
    Department,

    /// <summary>
    /// One row per cost center.
    /// </summary>
    CostCenter,
}
