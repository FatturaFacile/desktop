// <copyright file="WageTypeKind.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Payroll.Swiss;

/// <summary>
/// The kinds of wage types (values of PayrollWageType.Kind).
/// </summary>
public static class WageTypeKind
{
    /// <summary>
    /// Cash earning, part of the gross salary.
    /// </summary>
    public const string Earning = "Earning";

    /// <summary>
    /// Benefit in kind: part of the gross salary and of the bases, deducted again from the payout.
    /// </summary>
    public const string NonCash = "NonCash";

    /// <summary>
    /// Expense allowance: paid out, not part of the gross salary.
    /// </summary>
    public const string Expense = "Expense";

    /// <summary>
    /// Employee deduction.
    /// </summary>
    public const string Deduction = "Deduction";

    /// <summary>
    /// Employer contribution (not paid out, part of the employer cost).
    /// </summary>
    public const string Employer = "Employer";

    /// <summary>
    /// Information line (bases).
    /// </summary>
    public const string Information = "Information";

    /// <summary>
    /// Returns whether a kind is part of the gross salary (earnings and benefits in kind).
    /// </summary>
    /// <param name="kind">The kind.</param>
    /// <returns>True for gross salary kinds.</returns>
    public static bool IsGross(string? kind) =>
        string.Equals(kind, Earning, StringComparison.Ordinal) || string.Equals(kind, NonCash, StringComparison.Ordinal);

    /// <summary>
    /// Returns whether a kind is a contribution (employee deduction or employer contribution).
    /// </summary>
    /// <param name="kind">The kind.</param>
    /// <returns>True for contributions.</returns>
    public static bool IsContribution(string? kind) =>
        string.Equals(kind, Deduction, StringComparison.Ordinal) || string.Equals(kind, Employer, StringComparison.Ordinal);
}
