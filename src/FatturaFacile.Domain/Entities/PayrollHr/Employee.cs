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