using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.PayrollHr;

public sealed class PayrollSlipSummary : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public Currency Currency { get; private set; } = Currency.CHF;

    /// <summary>
    /// Gross earnings
    /// </summary>
    public decimal GrossSalary { get; private set; }

    /// <summary>
    /// Swiss deductions
    /// </summary>
    public decimal SwissAvsAiLppDeductions { get; private set; }

    /// <summary>
    /// Imposta alla fonte cantonale
    /// </summary>
    public decimal SwissSourceTaxWithheld { get; private set; }

    /// <summary>
    /// Italian deductions (if applicable for Frontaliere or Italian worker)
    /// </summary>
    public decimal ItalianInpsInailDeductions { get; private set; }
    public decimal ItalianIrpefAdvance { get; private set; }

    /// <summary>
    /// Net salary paid
    /// </summary>
    public decimal NetSalaryPaid { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private PayrollSlipSummary()
    {
    }

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