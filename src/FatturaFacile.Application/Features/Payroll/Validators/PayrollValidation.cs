// <copyright file="PayrollValidation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Constants;
using FatturaFacile.Application.Payroll.Swiss;

namespace FatturaFacile.Application.Features.Payroll.Validators;

/// <summary>
/// Allowed values shared by the payroll validators.
/// </summary>
public static class PayrollValidation
{
    /// <summary>
    /// Gets the residence permits (CH = Swiss citizen).
    /// </summary>
    public static IReadOnlyList<string> Permits { get; } = ["CH", "C", "B", "L", "G", "F", "N", "S", "Ci"];

    /// <summary>
    /// Gets the absence types.
    /// </summary>
    public static IReadOnlyList<string> AbsenceTypes { get; } =
        [AbsenceType.Sickness, AbsenceType.Accident, AbsenceType.Maternity, AbsenceType.Paternity, AbsenceType.Military, AbsenceType.Vacation, AbsenceType.Unpaid, AbsenceType.Other];

    /// <summary>
    /// Gets the wage type kinds.
    /// </summary>
    public static IReadOnlyList<string> WageTypeKinds { get; } =
        [WageTypeKind.Earning, WageTypeKind.NonCash, WageTypeKind.Expense, WageTypeKind.Deduction, WageTypeKind.Employer, WageTypeKind.Information];

    /// <summary>
    /// Gets the Swiss cantons.
    /// </summary>
    public static IReadOnlyList<string> Cantons { get; } =
    [
        "AG", "AI", "AR", "BE", "BL", "BS", "FR", "GE", "GL", "GR", "JU", "LU", "NE",
        "NW", "OW", "SG", "SH", "SO", "SZ", "TG", "TI", "UR", "VD", "VS", "ZG", "ZH",
    ];

    /// <summary>
    /// Returns whether a value is a Swiss canton code.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>True for a canton code.</returns>
    public static bool IsCanton(string? value) => value is not null && Cantons.Contains(value.ToUpperInvariant(), StringComparer.Ordinal);
}
