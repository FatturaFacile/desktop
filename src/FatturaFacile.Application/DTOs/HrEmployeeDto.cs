// <copyright file="HrEmployeeDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer object for an employee and compensation profile.
/// </summary>
public sealed class HrEmployeeDto
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the mandatory owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the employee number.
    /// </summary>
    public string EmployeeNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the social security number / AVS / CF.
    /// </summary>
    public string? SocialSecurityNumber { get; set; }

    /// <summary>
    /// Gets or sets the department name.
    /// </summary>
    public string? DepartmentName { get; set; }

    /// <summary>
    /// Gets or sets the job title.
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Gets or sets the employment status ("active", "onboarding", "suspended", "terminated").
    /// </summary>
    public string EmploymentStatus { get; set; } = "active";

    /// <summary>
    /// Gets or sets the employment type ("full_time", "part_time", "contractor", "hourly").
    /// </summary>
    public string EmploymentType { get; set; } = "full_time";

    /// <summary>
    /// Gets or sets the hire date.
    /// </summary>
    public DateTime HireDate { get; set; }

    /// <summary>
    /// Gets or sets the annual gross base salary (RAL).
    /// </summary>
    public decimal BaseAnnualSalary { get; set; }

    /// <summary>
    /// Gets or sets the standard hourly cost rate.
    /// </summary>
    public decimal HourlyCostRate { get; set; }

    /// <summary>
    /// Gets or sets the salary currency ("CHF", "EUR").
    /// </summary>
    public string Currency { get; set; } = "EUR";

    /// <summary>
    /// Gets or sets the FTE workload ratio (e.g. 1.0 = 100%).
    /// </summary>
    public decimal FteRatio { get; set; } = 1.0m;

    /// <summary>
    /// Gets or sets the work schedule template code.
    /// </summary>
    public string WorkScheduleCode { get; set; } = "STD-40H";

    /// <summary>
    /// Gets or sets the formatted full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;
}
