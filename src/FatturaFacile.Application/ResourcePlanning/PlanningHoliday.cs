// <copyright file="PlanningHoliday.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// A holiday that reduces the capacity: national, regional, municipal or tenant local.
/// </summary>
/// <param name="Date">The date.</param>
/// <param name="Name">The name.</param>
/// <param name="DayFraction">The share of the day that is off (1 or 0.5).</param>
public sealed record PlanningHoliday(
    DateOnly Date,
    string Name,
    decimal DayFraction);
