// <copyright file="SwissPayrollContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Payroll.Swiss;

/// <summary>
/// Everything the calculation of one payslip needs.
/// </summary>
public sealed class SwissPayrollContext
{
    /// <summary>
    /// Gets or sets the employee.
    /// </summary>
    public HrEmployee Employee { get; set; } = new();

    /// <summary>
    /// Gets or sets the company settings.
    /// </summary>
    public SwissPayrollSettings Settings { get; set; } = new();

    /// <summary>
    /// Gets or sets the year.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the month.
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// Gets or sets the wage types of the company by code.
    /// </summary>
    public IReadOnlyDictionary<string, PayrollWageType> WageTypes { get; set; } = new Dictionary<string, PayrollWageType>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the inputs of the employee for the month.
    /// </summary>
    public IReadOnlyList<PayrollInput> Inputs { get; set; } = [];

    /// <summary>
    /// Gets or sets the absences of the employee overlapping the month.
    /// </summary>
    public IReadOnlyList<HrAbsence> Absences { get; set; } = [];

    /// <summary>
    /// Gets or sets the children of the employee.
    /// </summary>
    public IReadOnlyList<HrEmployeeChild> Children { get; set; } = [];

    /// <summary>
    /// Gets or sets the family allowance rates (all cantons, all validity dates).
    /// </summary>
    public IReadOnlyList<FamilyAllowanceRate> FamilyAllowanceRates { get; set; } = [];

    /// <summary>
    /// Gets or sets the withholding tax tariff rows of the employee canton and code (all validity dates).
    /// </summary>
    public IReadOnlyList<WithholdingTaxTariff> WithholdingTaxTariffs { get; set; } = [];

    /// <summary>
    /// Gets or sets the year-to-date values.
    /// </summary>
    public SwissPayrollYearToDate YearToDate { get; set; } = SwissPayrollYearToDate.Empty;
}
