// <copyright file="PayrollSlipSummary.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.PayrollHr;

/// <summary>
/// Represents a monthly payroll slip computation summary (busta paga / Lohnabrechnung),
/// capturing gross compensation, social security contributions (INPS/INAIL or AVS/AI/LPP),
/// withholding taxes (IRPEF or Imposta alla fonte), and net disbursed pay for Italian, Swiss,
/// and cross-border Frontaliere personnel.
/// </summary>
public sealed class PayrollSlipSummary : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{

    /// <summary>
    /// Initializes a new instance of the <see cref="PayrollSlipSummary"/> class.
    /// </summary>
    private PayrollSlipSummary()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the employing tenant organization.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique identifier of the employee to whom this payslip pertains.
    /// </summary>
    public Guid EmployeeId { get; private set; }

    /// <summary>
    /// Gets the payroll reference calendar year (e.g., 2026).
    /// </summary>
    public int Year { get; private set; }

    /// <summary>
    /// Gets the payroll reference calendar month (1 through 12).
    /// </summary>
    public int Month { get; private set; }

    /// <summary>
    /// Gets the disbursement currency (EUR or CHF) used for this payroll calculation.
    /// </summary>
    public Currency Currency { get; private set; } = Currency.CHF;

    /// <summary>
    /// Gets the gross taxable compensation before social security deductions and tax withholding.
    /// </summary>
    public decimal GrossSalary { get; private set; }

    /// <summary>
    /// Gets the total Swiss mandatory social security and occupational pension contributions deducted from the worker (AVS/AHV, AI/IV, APG/EO, AC/ALV, and LPP/BVG 2nd Pillar).
    /// </summary>
    public decimal SwissAvsAiLppDeductions { get; private set; }

    /// <summary>
    /// Gets the cantonal Swiss source income withholding tax (Imposta alla fonte / Quellensteuer) deducted at source, particularly relevant for cross-border Frontalieri and foreign tax residents.
    /// </summary>
    public decimal SwissSourceTaxWithheld { get; private set; }

    /// <summary>
    /// Gets the total Italian employee mandatory social insurance contributions deducted (INPS social security and INAIL workplace accident insurance).
    /// </summary>
    public decimal ItalianInpsInailDeductions { get; private set; }

    /// <summary>
    /// Gets the Italian personal income tax (IRPEF) advance withheld at source by the employer as tax withholding agent (sostituto d'imposta), along with regional and municipal surtaxes.
    /// </summary>
    public decimal ItalianIrpefAdvance { get; private set; }

    /// <summary>
    /// Gets the final net take-home remuneration payable to the employee after all statutory deductions and withholdings.
    /// </summary>
    public decimal NetSalaryPaid { get; private set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the payroll record was calculated and created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or payroll system process that created the payslip record.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this payroll record was last modified, or <see langword="null"/> if never modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or process that last modified this payroll record.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates a payroll slip summary for an employee working in Switzerland under cross-border Frontaliere status,
    /// calculating net salary after Swiss social security (AVS/AI/LPP) and Swiss source tax (Imposta alla fonte).
    /// </summary>
    /// <param name="tenantId">The unique identifier of the employing tenant organization.</param>
    /// <param name="employeeId">The unique identifier of the employee.</param>
    /// <param name="year">The payroll reference calendar year.</param>
    /// <param name="month">The payroll reference calendar month (1-12).</param>
    /// <param name="grossChf">The gross remuneration amount in Swiss Francs.</param>
    /// <param name="avsAiLppChf">The total Swiss social security and 2nd pillar pension deductions in Swiss Francs.</param>
    /// <param name="sourceTaxChf">The cantonal source withholding tax deducted in Swiss Francs.</param>
    /// <returns>A configured <see cref="PayrollSlipSummary"/> entity with net salary computed in CHF.</returns>
    public static PayrollSlipSummary CreateSwissFrontaliere(
        Guid    tenantId,
        Guid    employeeId,
        int     year,
        int     month,
        decimal grossChf,
        decimal avsAiLppChf,
        decimal sourceTaxChf)
    {
        var net = grossChf - avsAiLppChf - sourceTaxChf;

        return new PayrollSlipSummary
               {
                   Id                         = Guid.NewGuid(),
                   TenantId                   = tenantId,
                   EmployeeId                 = employeeId,
                   Year                       = year,
                   Month                      = month,
                   Currency                   = Currency.CHF,
                   GrossSalary                = grossChf,
                   SwissAvsAiLppDeductions    = avsAiLppChf,
                   SwissSourceTaxWithheld     = sourceTaxChf,
                   ItalianInpsInailDeductions = 0m,
                   ItalianIrpefAdvance        = 0m,
                   NetSalaryPaid              = net,
                   CreatedAtUtc               = DateTime.UtcNow,
               };
    }

    /// <summary>
    /// Creates a payroll slip summary for an employee under Italian employment jurisdiction,
    /// calculating net pay after Italian statutory social contributions (INPS/INAIL) and income tax withholding (IRPEF).
    /// </summary>
    /// <param name="tenantId">The unique identifier of the employing tenant organization.</param>
    /// <param name="employeeId">The unique identifier of the employee.</param>
    /// <param name="year">The payroll reference calendar year.</param>
    /// <param name="month">The payroll reference calendar month (1-12).</param>
    /// <param name="grossEur">The gross taxable earnings in Euros.</param>
    /// <param name="inpsInailEur">The mandatory INPS social security and INAIL insurance deductions in Euros.</param>
    /// <param name="irpefEur">The personal income tax (IRPEF) withholding amount in Euros.</param>
    /// <returns>A configured <see cref="PayrollSlipSummary"/> entity with net salary computed in EUR.</returns>
    public static PayrollSlipSummary CreateItalian(
        Guid    tenantId,
        Guid    employeeId,
        int     year,
        int     month,
        decimal grossEur,
        decimal inpsInailEur,
        decimal irpefEur)
    {
        var net = grossEur - inpsInailEur - irpefEur;

        return new PayrollSlipSummary
               {
                   Id                         = Guid.NewGuid(),
                   TenantId                   = tenantId,
                   EmployeeId                 = employeeId,
                   Year                       = year,
                   Month                      = month,
                   Currency                   = Currency.EUR,
                   GrossSalary                = grossEur,
                   SwissAvsAiLppDeductions    = 0m,
                   SwissSourceTaxWithheld     = 0m,
                   ItalianInpsInailDeductions = inpsInailEur,
                   ItalianIrpefAdvance        = irpefEur,
                   NetSalaryPaid              = net,
                   CreatedAtUtc               = DateTime.UtcNow,
               };
    }

}
