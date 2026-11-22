// <copyright file="LocalHolidayRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The body that adds a local holiday.
/// </summary>
/// <param name="Country">The country code, CH or IT.</param>
/// <param name="RegionCode">The region code.</param>
/// <param name="Municipality">The municipality.</param>
/// <param name="Month">The month.</param>
/// <param name="Day">The day.</param>
/// <param name="Year">The year, or null to repeat every year.</param>
/// <param name="Name">The name.</param>
/// <param name="DayFraction">The share of the day that is off.</param>
public sealed record LocalHolidayRequest(
    string Country,
    string? RegionCode,
    string? Municipality,
    int Month,
    int Day,
    int? Year,
    string Name,
    decimal DayFraction);
