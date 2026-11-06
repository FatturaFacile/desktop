// <copyright file="SwissPayrollYearToDate.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Payroll.Swiss;

/// <summary>
/// Cumulated values of the previous months of the same year (closed or draft runs), for the annual ceilings
/// and the 13th salary.
/// </summary>
/// <param name="PaidDays">Paid days before the current month (30-day convention).</param>
/// <param name="AlvBasis">ALV basis already used.</param>
/// <param name="UvgBasis">UVG basis already used.</param>
public sealed record SwissPayrollYearToDate(decimal PaidDays, decimal AlvBasis, decimal UvgBasis)
{
    /// <summary>
    /// Gets the empty year-to-date values (first month).
    /// </summary>
    public static SwissPayrollYearToDate Empty { get; } = new(0m, 0m, 0m);
}
