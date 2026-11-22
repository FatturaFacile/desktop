// <copyright file="HolidayRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One holiday of the resolved list of a place and year, ready to show.
/// </summary>
/// <param name="DateText">The date.</param>
/// <param name="Name">The name.</param>
/// <param name="FractionText">The part of the day that is off.</param>
internal sealed record HolidayRow(string DateText, string Name, string FractionText);
