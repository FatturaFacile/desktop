// <copyright file="PayrollCalculationResultDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer object representing the breakdown of monthly payroll compensation and statutory deductions.
/// </summary>
public sealed class PayrollCalculationResultDto
{
    /// <summary>
    /// Gets or sets the country code ("CH" or "IT").
    /// </summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payroll currency ("CHF" or "EUR").
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the gross monthly compensation before any deductions.
    /// </summary>
    public decimal GrossMonthlySalary { get; set; }

    /// <summary>
    /// Gets or sets the total statutory deductions from gross salary.
    /// </summary>
    public decimal TotalDeductions { get; set; }

    /// <summary>
    /// Gets or sets the net take-home salary paid to employee.
    /// </summary>
    public decimal NetMonthlySalary { get; set; }

    /// <summary>
    /// Gets or sets the employer social contribution cost.
    /// </summary>
    public decimal EmployerTotalCost { get; set; }

    /// <summary>
    /// Gets or sets the itemized breakdown of deductions (e.g., AVS/AI, AC, LPP, QST or INPS, IRPEF).
    /// </summary>
    public IDictionary<string, decimal> DeductionBreakdown { get; set; } = new Dictionary<string, decimal>();

    /// <summary>
    /// Gets or sets legal and compliance notes (e.g. Swissdec ELM reference, CCNL reference).
    /// </summary>
    public string ComplianceNotes { get; set; } = string.Empty;
}
