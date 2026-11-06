// <copyright file="ItalianPayrollContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Payroll.Italian;

/// <summary>
/// Input of the Italian monthly payroll of one employee.
/// </summary>
public sealed class ItalianPayrollContext
{
    /// <summary>
    /// Gets the employee.
    /// </summary>
    public required HrEmployee Employee { get; init; }

    /// <summary>
    /// Gets the payroll year.
    /// </summary>
    public required int Year { get; init; }

    /// <summary>
    /// Gets the payroll month.
    /// </summary>
    public required int Month { get; init; }

    /// <summary>
    /// Gets the company settings.
    /// </summary>
    public ItalianPayrollSettings? Settings { get; init; }

    /// <summary>
    /// Gets the collective agreement of the employee.
    /// </summary>
    public CollectiveAgreement? Agreement { get; init; }

    /// <summary>
    /// Gets the agreement level of the employee.
    /// </summary>
    public CollectiveAgreementLevel? Level { get; init; }

    /// <summary>
    /// Gets the variable inputs of the month.
    /// </summary>
    public IReadOnlyList<PayrollInput> Inputs { get; init; } = [];

    /// <summary>
    /// Gets the absences overlapping the month.
    /// </summary>
    public IReadOnlyList<HrAbsence> Absences { get; init; } = [];

    /// <summary>
    /// Gets the payslips of the same year before this month.
    /// </summary>
    public IReadOnlyList<Payslip> YearToDate { get; init; } = [];

    /// <summary>
    /// Gets the lines of the payslips of the same year before this month.
    /// </summary>
    public IReadOnlyList<PayslipLine> YearToDateLines { get; init; } = [];

    /// <summary>
    /// Gets the taxable income of the previous year (basis of the IRPEF surcharges withheld this year).
    /// </summary>
    public decimal PreviousYearTaxable { get; init; }
}
