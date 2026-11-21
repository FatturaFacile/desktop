// <copyright file="RoutingCalculation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Manufacturing;

/// <summary>
/// The lead time and cost of a routing for a quantity.
/// </summary>
/// <param name="Quantity">The produced quantity.</param>
/// <param name="IncludeOverlap">Whether operation overlap was applied.</param>
/// <param name="TotalMinutes">The lead time in minutes.</param>
/// <param name="SerialMinutes">The lead time without overlap in minutes.</param>
/// <param name="OverlapSavingMinutes">The minutes saved by the overlap.</param>
/// <param name="Cost">The total cost.</param>
/// <param name="UnitCost">The cost per unit.</param>
/// <param name="Steps">The steps.</param>
public sealed record RoutingCalculation(
    decimal Quantity,
    bool IncludeOverlap,
    double TotalMinutes,
    double SerialMinutes,
    double OverlapSavingMinutes,
    RoutingCostBreakdown Cost,
    decimal UnitCost,
    IReadOnlyList<RoutingStepResult> Steps);
