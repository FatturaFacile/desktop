using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.PayrollHr;

public sealed class Employee : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    /// <summary>
    /// EMP-001
    /// </summary>
    public string EmployeeCode { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTime DateOfBirth { get; private set; }
    public WorkerType WorkerType { get; private set; } = WorkerType.Employee;

    /// <summary>
    /// Cross-Border status (Frontaliere IT-CH)
    /// </summary>
    public bool IsFrontaliereCrossBorder { get; private set; }
    /// <summary>
    /// TI, GR, VS
    /// </summary>
    public string? CantonOfEmployment { get; private set; }
    public string FiscalResidenceCountry { get; private set; } = "IT";

    /// <summary>
    /// Tax Identifiers
    /// </summary>
    public FiscalCode? ItalianFiscalCode { get; private set; }
    /// <summary>
    /// 756.xxxx.xxxx.xx
    /// </summary>
    public string? SwissAhvAvsNumber { get; private set; }
    public Address HomeAddress { get; private set; } = null!;

    public string JobTitle { get; private set; } = string.Empty;
    public decimal MonthlyGrossSalary { get; private set; }
    public Currency SalaryCurrency { get; private set; } = Currency.EUR;
    public bool IsActive { get; } = true;

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private Employee()
    {
    }

    public static Employee Create(
        Guid tenantId,
        string code,
        string firstName,
        string lastName,
        DateTime dob,
        WorkerType workerType,
        bool isFrontaliere,
        Address address,
        string jobTitle,
        decimal salary,
        Currency currency,
        string? canton = null,
        FiscalCode? cf = null,
        string? avs = null)
    {
        return new Employee
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeCode = code.Trim().ToUpperInvariant(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            DateOfBirth = dob,
            WorkerType = workerType,
            IsFrontaliereCrossBorder = isFrontaliere,
            CantonOfEmployment = canton?.Trim().ToUpperInvariant(),
            FiscalResidenceCountry = isFrontaliere ? "IT" : (canton != null ? "CH" : "IT"),
            HomeAddress = address,
            JobTitle = jobTitle.Trim(),
            MonthlyGrossSalary = salary,
            SalaryCurrency = currency,
            ItalianFiscalCode = cf,
            SwissAhvAvsNumber = avs?.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}

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
