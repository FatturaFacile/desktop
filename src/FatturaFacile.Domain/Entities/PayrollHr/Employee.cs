// <copyright file="Employee.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.PayrollHr;

/// <summary>
/// Represents an employee profile with cross-border Italian-Swiss payroll details,
/// tracking statutory identification numbers, canton of employment, Frontaliere commuter status, and compensation terms.
/// </summary>
public sealed class Employee : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{

    /// <summary>
    /// Initializes a new instance of the <see cref="Employee"/> class.
    /// </summary>
    private Employee()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization employing this individual.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique internal employee code or staff personnel number (e.g., &quot;EMP-001&quot;).
    /// </summary>
    public string EmployeeCode { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the given name of the employee.
    /// </summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the surname or family name of the employee.
    /// </summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the calendar date of birth of the employee.
    /// </summary>
    public DateTime DateOfBirth { get; private set; }

    /// <summary>
    /// Gets the worker employment classification category (e.g., Employee, Contractor, Executive).
    /// </summary>
    public WorkerType WorkerType { get; private set; } = WorkerType.Employee;

    /// <summary>
    /// Gets a value indicating whether the employee holds Italian-Swiss cross-border commuter status (Frontaliere IT-CH / Grenzgänger) under the bilateral tax treaties.
    /// </summary>
    public bool IsFrontaliereCrossBorder { get; private set; }

    /// <summary>
    /// Gets the Swiss canton of employment (e.g., &quot;TI&quot; for Ticino, &quot;GR&quot; for Graubünden, &quot;VS&quot; for Valais) if working in Switzerland; otherwise <see langword="null"/>.
    /// </summary>
    public string? CantonOfEmployment { get; private set; }

    /// <summary>
    /// Gets the ISO 3166-1 alpha-2 country code of primary fiscal and personal tax residence (&quot;IT&quot; or &quot;CH&quot;).
    /// </summary>
    public string FiscalResidenceCountry { get; private set; } = "IT";

    /// <summary>
    /// Gets the Italian fiscal code (Codice Fiscale) for social security and income tax withholding, if assigned.
    /// </summary>
    public FiscalCode? ItalianFiscalCode { get; private set; }

    /// <summary>
    /// Gets the 13-digit Swiss social security / AHV/AVS identification number (format 756.xxxx.xxxx.xx), if assigned.
    /// </summary>
    public string? SwissAhvAvsNumber { get; private set; }

    /// <summary>
    /// Gets the primary residential physical address of the employee.
    /// </summary>
    public Address HomeAddress { get; private set; } = null!;

    /// <summary>
    /// Gets the employee's formal occupational title or role description.
    /// </summary>
    public string JobTitle { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the contracted monthly gross salary base before statutory deductions and taxes.
    /// </summary>
    public decimal MonthlyGrossSalary { get; private set; }

    /// <summary>
    /// Gets the contractual remuneration currency (EUR or CHF).
    /// </summary>
    public Currency SalaryCurrency { get; private set; } = Currency.EUR;

    /// <summary>
    /// Gets a value indicating whether the employee is actively employed and on the payroll.
    /// </summary>
    public bool IsActive { get; } = true;

    /// <summary>
    /// Gets or sets the UTC timestamp when the employee record was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or process that created this employee record.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this employee record was last modified, or <see langword="null"/> if never modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or process that last modified this employee record.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates and initializes a new <see cref="Employee"/> aggregate root instance, establishing fiscal residence based on cross-border Frontaliere status and employment canton.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the employing tenant organization.</param>
    /// <param name="code">The employee internal staff personnel code.</param>
    /// <param name="firstName">The employee's given name.</param>
    /// <param name="lastName">The employee's surname.</param>
    /// <param name="dob">The employee's date of birth.</param>
    /// <param name="workerType">The worker employment category classification.</param>
    /// <param name="isFrontaliere">A flag indicating whether the worker has Italian-Swiss cross-border commuter status.</param>
    /// <param name="address">The residential home address of the employee.</param>
    /// <param name="jobTitle">The professional job title.</param>
    /// <param name="salary">The contractual monthly gross salary base.</param>
    /// <param name="currency">The remuneration currency (EUR or CHF).</param>
    /// <param name="canton">The optional two-letter Swiss canton code of employment.</param>
    /// <param name="cf">The optional Italian Codice Fiscale.</param>
    /// <param name="avs">The optional Swiss AHV/AVS social security number (756.xxxx.xxxx.xx).</param>
    /// <returns>A fully configured <see cref="Employee"/> aggregate root entity.</returns>
    public static Employee Create(
        Guid        tenantId,
        string      code,
        string      firstName,
        string      lastName,
        DateTime    dob,
        WorkerType  workerType,
        bool        isFrontaliere,
        Address     address,
        string      jobTitle,
        decimal     salary,
        Currency    currency,
        string?     canton = null,
        FiscalCode? cf     = null,
        string?     avs    = null)
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
                   Id                       = Guid.NewGuid(),
                   TenantId                 = tenantId,
                   EmployeeCode             = code?.Trim().ToUpperInvariant() ?? string.Empty,
                   FirstName                = firstName?.Trim()               ?? string.Empty,
                   LastName                 = lastName?.Trim()                ?? string.Empty,
                   DateOfBirth              = dob,
                   WorkerType               = workerType,
                   IsFrontaliereCrossBorder = isFrontaliere,
                   CantonOfEmployment       = canton?.Trim().ToUpperInvariant(),
                   FiscalResidenceCountry   = residenceCountry,
                   HomeAddress              = address,
                   JobTitle                 = jobTitle?.Trim() ?? string.Empty,
                   MonthlyGrossSalary       = salary,
                   SalaryCurrency           = currency,
                   ItalianFiscalCode        = cf,
                   SwissAhvAvsNumber        = avs?.Trim() ?? string.Empty,
                   CreatedAtUtc             = DateTime.UtcNow,
               };
    }

}
