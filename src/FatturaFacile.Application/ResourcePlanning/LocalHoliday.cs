// <copyright file="LocalHoliday.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// A holiday the tenant added for a country, region or municipality.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="Country">The country code, CH or IT.</param>
/// <param name="RegionCode">The region code.</param>
/// <param name="Municipality">The municipality.</param>
/// <param name="Month">The month.</param>
/// <param name="Day">The day.</param>
/// <param name="Year">The year, or null when it repeats every year.</param>
/// <param name="Name">The name.</param>
/// <param name="DayFraction">The share of the day that is off.</param>
public sealed record LocalHoliday(
    Guid Id,
    string Country,
    string? RegionCode,
    string? Municipality,
    int Month,
    int Day,
    int? Year,
    string Name,
    decimal DayFraction);
