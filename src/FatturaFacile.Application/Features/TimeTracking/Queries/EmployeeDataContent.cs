// <copyright file="EmployeeDataContent.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Features.TimeTracking.Queries;

/// <summary>
/// The data collected for an employee export.
/// </summary>
internal sealed class EmployeeDataContent
{
    /// <summary>
    /// Gets the employee.
    /// </summary>
    public required HrEmployee Employee { get; init; }

    /// <summary>
    /// Gets the company name.
    /// </summary>
    public required string CompanyName { get; init; }

    /// <summary>
    /// Gets the family members.
    /// </summary>
    public IReadOnlyList<HrEmployeeChild> Children { get; init; } = [];

    /// <summary>
    /// Gets the absences.
    /// </summary>
    public IReadOnlyList<HrAbsence> Absences { get; init; } = [];

    /// <summary>
    /// Gets the clock events (also the logically deleted ones).
    /// </summary>
    public IReadOnlyList<TimeClockEvent> ClockEvents { get; init; } = [];

    /// <summary>
    /// Gets the daily records.
    /// </summary>
    public IReadOnlyList<TimeDailyRecord> DailyRecords { get; init; } = [];

    /// <summary>
    /// Gets the change history.
    /// </summary>
    public IReadOnlyList<TimeAuditEntry> Audit { get; init; } = [];

    /// <summary>
    /// Gets the evaluation of the recorded period.
    /// </summary>
    public WorkingTimeReport? Report { get; init; }

    /// <summary>
    /// Gets a value indicating whether the payroll data are included.
    /// </summary>
    public bool IncludesPayroll { get; init; }

    /// <summary>
    /// Gets the payslips.
    /// </summary>
    public IReadOnlyList<Payslip> Payslips { get; init; } = [];

    /// <summary>
    /// Gets the payslip lines.
    /// </summary>
    public IReadOnlyList<PayslipLine> PayslipLines { get; init; } = [];

    /// <summary>
    /// Gets the variable payroll inputs.
    /// </summary>
    public IReadOnlyList<PayrollInput> PayrollInputs { get; init; } = [];

    /// <summary>
    /// Gets the salary certificates.
    /// </summary>
    public IReadOnlyList<SalaryCertificate> SalaryCertificates { get; init; } = [];
}
