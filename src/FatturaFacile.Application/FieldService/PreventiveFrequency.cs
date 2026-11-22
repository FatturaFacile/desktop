// <copyright file="PreventiveFrequency.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// The preventive maintenance rule of a contract: a calendar interval and/or a meter threshold, with a lead time.
/// </summary>
/// <param name="CalendarEvery">The calendar interval length.</param>
/// <param name="CalendarUnit">The calendar unit: day, week, month or year.</param>
/// <param name="OperatingHours">The operating hours threshold.</param>
/// <param name="Cycles">The cycle threshold.</param>
/// <param name="LeadPercent">The meter lead, as a percentage of the threshold.</param>
/// <param name="LeadDays">The lead time in days.</param>
public sealed record PreventiveFrequency(int? CalendarEvery, string? CalendarUnit, decimal? OperatingHours, int? Cycles, decimal? LeadPercent, int? LeadDays);
