// <copyright file="Payslip.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// The payslip of an employee in a payroll run, with the bases of every insurance and the totals.
/// </summary>
public class Payslip : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the company (tenant) identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the payroll run identifier.
    /// </summary>
    public Guid PayrollRunId { get; set; }

    /// <summary>
    /// Gets or sets the employee identifier.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Gets or sets the year.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the month.
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// Gets or sets the employee name at calculation time.
    /// </summary>
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the paid days (30-day month convention).
    /// </summary>
    public decimal PaidDays { get; set; } = 30m;

    /// <summary>
    /// Gets or sets the gross salary.
    /// </summary>
    public decimal GrossSalary { get; set; }

    /// <summary>
    /// Gets or sets the AHV basis.
    /// </summary>
    public decimal AhvBasis { get; set; }

    /// <summary>
    /// Gets or sets the ALV basis (capped).
    /// </summary>
    public decimal AlvBasis { get; set; }

    /// <summary>
    /// Gets or sets the UVG basis (capped).
    /// </summary>
    public decimal UvgBasis { get; set; }

    /// <summary>
    /// Gets or sets the KTG basis.
    /// </summary>
    public decimal KtgBasis { get; set; }

    /// <summary>
    /// Gets or sets the annual BVG coordinated (insured) salary.
    /// </summary>
    public decimal BvgInsuredSalary { get; set; }

    /// <summary>
    /// Gets or sets the withholding tax basis.
    /// </summary>
    public decimal WithholdingTaxBasis { get; set; }

    /// <summary>
    /// Gets or sets the applied withholding tax rate.
    /// </summary>
    public decimal WithholdingTaxRatePercent { get; set; }

    /// <summary>
    /// Gets or sets the applied withholding tax tariff code.
    /// </summary>
    public string? WithholdingTaxCode { get; set; }

    /// <summary>
    /// Gets or sets the canton entitled to the withholding tax.
    /// </summary>
    public string? WithholdingTaxCanton { get; set; }

    /// <summary>
    /// Gets or sets the total employee deductions.
    /// </summary>
    public decimal EmployeeDeductions { get; set; }

    /// <summary>
    /// Gets or sets the net salary.
    /// </summary>
    public decimal NetSalary { get; set; }

    /// <summary>
    /// Gets or sets the amount paid out (net + expenses - benefits in kind).
    /// </summary>
    public decimal PayoutAmount { get; set; }

    /// <summary>
    /// Gets or sets the total employer contributions.
    /// </summary>
    public decimal EmployerContributions { get; set; }

    /// <summary>
    /// Gets or sets calculation warnings (missing tariffs, settings).
    /// </summary>
    public string? Warnings { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
