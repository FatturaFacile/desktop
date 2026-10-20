using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.PayrollHr;

/// <summary>
/// Represents an employee record with cross-border Italian-Swiss payroll details.
/// </summary>
public sealed class Employee : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the unique internal employee code (e.g. EMP-001).
    /// </summary>
    public string EmployeeCode { get; private set; } = string.Empty;

/// <summary>
    /// Gets the employee's given name.
    /// </summary>
    public string FirstName { get; private set; } = string.Empty;

/// <summary>
    /// Gets the employee's family name.
    /// </summary>
    public string LastName { get; private set; } = string.Empty;

/// <summary>
    /// Gets the employee's date of birth.
    /// </summary>
    public DateTime DateOfBirth { get; private set; }

/// <summary>
    /// Gets the worker employment category.
    /// </summary>
    public WorkerType WorkerType { get; private set; } = WorkerType.Employee;

/// <summary>
    /// Gets a value indicating whether the employee has cross-border commuter status (Frontaliere IT-CH).
    /// </summary>
    public bool IsFrontaliereCrossBorder { get; private set; }

/// <summary>
    /// Gets the Swiss canton of employment (e.g. TI, GR, VS), if applicable.
    /// </summary>
    public string? CantonOfEmployment { get; private set; }

/// <summary>
    /// Gets the ISO country code of fiscal residence.
    /// </summary>
    public string FiscalResidenceCountry { get; private set; } = "IT";

/// <summary>
    /// Gets the Italian fiscal code, if assigned.
    /// </summary>
    public FiscalCode? ItalianFiscalCode { get; private set; }

/// <summary>
    /// Gets the Swiss social security / AHV/AVS number (756.xxxx.xxxx.xx), if assigned.
    /// </summary>
    public string? SwissAhvAvsNumber { get; private set; }

/// <summary>
    /// Gets the residential home address.
    /// </summary>
    public Address HomeAddress { get; private set; } = null!;

/// <summary>
    /// Gets the primary job title or role description.
    /// </summary>
    public string JobTitle { get; private set; } = string.Empty;

/// <summary>
    /// Gets the monthly gross remuneration amount.
    /// </summary>
    public decimal MonthlyGrossSalary { get; private set; }

/// <summary>
    /// Gets the salary currency.
    /// </summary>
    public Currency SalaryCurrency { get; private set; } = Currency.EUR;

/// <summary>
    /// Gets a value indicating whether the employee is currently active.
    /// </summary>
    public bool IsActive { get; } = true;

/// <summary>
    /// Gets or sets the UTC timestamp when the record was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

/// <summary>
    /// Gets or sets the user or system identity that created the record.
    /// </summary>
    public string? CreatedBy { get; set; }

/// <summary>
    /// Gets or sets the UTC timestamp when the record was last updated.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

/// <summary>
    /// Gets or sets the user or system identity that last updated the record.
    /// </summary>
    public string? UpdatedBy { get; set; }

private Employee()
    {
    }

/// <summary>
    /// Creates and initializes a new <see cref="Employee"/> instance.
    /// </summary>
    /// <param name="tenantId">The associated tenant identifier.</param>
    /// <param name="code">The employee code.</param>
    /// <param name="firstName">The given name.</param>
    /// <param name="lastName">The family name.</param>
    /// <param name="dob">The date of birth.</param>
    /// <param name="workerType">The worker classification.</param>
    /// <param name="isFrontaliere">Indicates cross-border commuter status.</param>
    /// <param name="address">The residential address.</param>
    /// <param name="jobTitle">The professional job title.</param>
    /// <param name="salary">The gross monthly remuneration.</param>
    /// <param name="currency">The compensation currency.</param>
    /// <param name="canton">The optional employment canton.</param>
    /// <param name="cf">The optional Italian fiscal code.</param>
    /// <param name="avs">The optional Swiss AHV/AVS identifier.</param>
    /// <returns>A fully initialized <see cref="Employee"/> entity.</returns>
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
        string residenceCountry;
        if (isFrontaliere)
        {
            residenceCountry = "IT";
        }
        else
        {
            residenceCountry = canton is not null ? "CH" : "IT";
        }

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
            FiscalResidenceCountry = residenceCountry,
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
