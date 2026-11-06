// <copyright file="PayrollLookups.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using FatturaFacile.Application.Features.Payroll.Validators;

namespace FatturaFacile.Wpf.ViewModels.Payroll;

/// <summary>
/// Values offered by the payroll combo boxes.
/// </summary>
[SuppressMessage("Design", "MA0182:Avoid unused internal types", Justification = "Used from XAML.")]
internal static class PayrollLookups
{
    /// <summary>
    /// Gets the cantons.
    /// </summary>
    public static IReadOnlyList<string> Cantons => PayrollValidation.Cantons;

    /// <summary>
    /// Gets the residence permits.
    /// </summary>
    public static IReadOnlyList<string> Permits => PayrollValidation.Permits;

    /// <summary>
    /// Gets the absence types.
    /// </summary>
    public static IReadOnlyList<string> AbsenceTypes => PayrollValidation.AbsenceTypes;

    /// <summary>
    /// Gets the wage type kinds.
    /// </summary>
    public static IReadOnlyList<string> WageTypeKinds => PayrollValidation.WageTypeKinds;

    /// <summary>
    /// Gets the pay types.
    /// </summary>
    public static IReadOnlyList<string> PayTypes { get; } = ["monthly", "hourly"];

    /// <summary>
    /// Gets the genders.
    /// </summary>
    public static IReadOnlyList<string> Genders { get; } = ["M", "F"];

    /// <summary>
    /// Gets the civil statuses.
    /// </summary>
    public static IReadOnlyList<string> CivilStatuses { get; } = ["single", "married", "divorced", "widowed", "registered_partnership", "separated"];

    /// <summary>
    /// Gets the months.
    /// </summary>
    public static IReadOnlyList<int> Months { get; } = [.. Enumerable.Range(1, 12)];

    /// <summary>
    /// Gets the selectable years (two years back, one ahead).
    /// </summary>
    public static IReadOnlyList<int> Years { get; } = [.. Enumerable.Range(DateTime.Today.Year - 2, 4)];
}
