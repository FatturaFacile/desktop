// <copyright file="OverloadWeek.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The load of one week after a mutation.
/// </summary>
/// <param name="WeekStart">The Monday of the week.</param>
/// <param name="LoadPercent">The load in percent.</param>
public sealed record OverloadWeek(
    DateOnly WeekStart,
    double LoadPercent);
