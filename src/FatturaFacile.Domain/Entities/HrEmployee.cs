// <copyright file="HrEmployee.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents an employee record with personnel and payroll compensation metadata.
/// </summary>
public class HrEmployee : ITenantScopedEntity
{

    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the mandatory owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the employee internal badge or serial number (e.g., "EMP-001").
    /// </summary>
    public string EmployeeNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the employee given first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the employee family last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the corporate email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the mobile or phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the National Insurance Number / AVS No (Switzerland) or Codice Fiscale (Italy).
    /// </summary>
    public string? SocialSecurityNumber { get; set; }

    /// <summary>
    /// Gets or sets the department or organizational unit name.
    /// </summary>
    public string? DepartmentName { get; set; }

    /// <summary>
    /// Gets or sets the organizational role or job title.
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Gets or sets the employment status ("active", "onboarding", "suspended", "terminated").
    /// </summary>
    public string EmploymentStatus { get; set; } = "active";

    /// <summary>
    /// Gets or sets the employment classification ("full_time", "part_time", "contractor", "hourly").
    /// </summary>
    public string EmploymentType { get; set; } = "full_time";

    /// <summary>
    /// Gets or sets the start date of employment.
    /// </summary>
    public DateTime HireDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the contract termination date if applicable.
    /// </summary>
    public DateTime? TerminationDate { get; set; }

    /// <summary>
    /// Gets or sets the annual gross compensation base salary (RAL).
    /// </summary>
    public decimal BaseAnnualSalary { get; set; }

    /// <summary>
    /// Gets or sets the standard hourly cost rate.
    /// </summary>
    public decimal HourlyCostRate { get; set; }

    /// <summary>
    /// Gets or sets the payroll compensation currency ("CHF", "EUR").
    /// </summary>
    public string Currency { get; set; } = "EUR";

    /// <summary>
    /// Gets or sets the Full-Time Equivalent workload ratio (e.g. 1.0 for 100%, 0.8 for 80%).
    /// </summary>
    public decimal FteRatio { get; set; } = 1.0m;

    /// <summary>
    /// Gets or sets the work schedule template code (e.g. "STD-40H", "STD-42H").
    /// </summary>
    public string WorkScheduleCode { get; set; } = "STD-40H";

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the soft delete timestamp.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets the formatted full name of the employee.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
}
