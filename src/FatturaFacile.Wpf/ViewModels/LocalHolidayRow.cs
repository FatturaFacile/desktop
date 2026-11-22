// <copyright file="LocalHolidayRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One local holiday of the tenant, ready to show.
/// </summary>
/// <param name="Holiday">The holiday.</param>
/// <param name="ScopeText">The country, region and municipality it applies to.</param>
/// <param name="DateText">The day of the year, or the date when it is not yearly.</param>
/// <param name="FractionText">The part of the day that is off.</param>
internal sealed record LocalHolidayRow(LocalHoliday Holiday, string ScopeText, string DateText, string FractionText);
