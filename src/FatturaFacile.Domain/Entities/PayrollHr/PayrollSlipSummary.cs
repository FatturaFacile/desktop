// <copyright file="PayrollSlipSummary.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.PayrollHr;

/// <summary>
/// Represents payroll slip summary.
/// </summary>
public sealed class PayrollSlipSummary : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private PayrollSlipSummary()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the employee id.
    /// </summary>
    public Guid EmployeeId { get; private set; }

    /// <summary>
    /// Gets the year.
    /// </summary>
    public int Year { get; private set; }

    /// <summary>
    /// Gets the month.
    /// </summary>
    public int Month { get; private set; }

    /// <summary>
    /// Gets the currency.
    /// </summary>
    public Currency Currency { get; private set; } = Currency.CHF;

    /// <summary>
    /// Gets the gross earnings.
    /// </summary>
    public decimal GrossSalary { get; private set; }

    /// <summary>
    /// Gets the swiss deductions.
    /// </summary>
    public decimal SwissAvsAiLppDeductions { get; private set; }

    /// <summary>
    /// Gets the imposta alla fonte cantonale.
    /// </summary>
    public decimal SwissSourceTaxWithheld { get; private set; }

    /// <summary>
    /// Gets the italian deductions (if applicable for Frontaliere or Italian worker).
    /// </summary>
    public decimal ItalianInpsInailDeductions { get; private set; }

    /// <summary>
    /// Gets the italian irpef advance.
    /// </summary>
    public decimal ItalianIrpefAdvance { get; private set; }

    /// <summary>
    /// Gets the net salary paid.
    /// </summary>
    public decimal NetSalaryPaid { get; private set; }

    /// <summary>
    /// Gets or sets the created at utc.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the created by.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the updated at utc.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the updated by.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Create Swiss Frontaliere.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="employeeId">The employee id.</param>
    /// <param name="year">The year.</param>
    /// <param name="month">The month.</param>
    /// <param name="grossChf">The gross chf.</param>
    /// <param name="avsAiLppChf">The avs ai lpp chf.</param>
    /// <param name="sourceTaxChf">The source tax chf.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static PayrollSlipSummary CreateSwissFrontaliere(
        Guid tenantId,
        Guid employeeId,
        int year,
        int month,
        decimal grossChf,
        decimal avsAiLppChf,
        decimal sourceTaxChf)
    {
        var net = grossChf - avsAiLppChf - sourceTaxChf;
        return new PayrollSlipSummary
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = employeeId,
            Year = year,
            Month = month,
            Currency = Currency.CHF,
            GrossSalary = grossChf,
            SwissAvsAiLppDeductions = avsAiLppChf,
            SwissSourceTaxWithheld = sourceTaxChf,
            ItalianInpsInailDeductions = 0m,
            ItalianIrpefAdvance = 0m,
            NetSalaryPaid = net,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Create Italian.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="employeeId">The employee id.</param>
    /// <param name="year">The year.</param>
    /// <param name="month">The month.</param>
    /// <param name="grossEur">The gross eur.</param>
    /// <param name="inpsInailEur">The inps inail eur.</param>
    /// <param name="irpefEur">The irpef eur.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static PayrollSlipSummary CreateItalian(
        Guid tenantId,
        Guid employeeId,
        int year,
        int month,
        decimal grossEur,
        decimal inpsInailEur,
        decimal irpefEur)
    {
        var net = grossEur - inpsInailEur - irpefEur;
        return new PayrollSlipSummary
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = employeeId,
            Year = year,
            Month = month,
            Currency = Currency.EUR,
            GrossSalary = grossEur,
            SwissAvsAiLppDeductions = 0m,
            SwissSourceTaxWithheld = 0m,
            ItalianInpsInailDeductions = inpsInailEur,
            ItalianIrpefAdvance = irpefEur,
            NetSalaryPaid = net,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
