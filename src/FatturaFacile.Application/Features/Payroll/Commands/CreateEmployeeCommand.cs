// <copyright file="CreateEmployeeCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Commands;

/// <summary>
/// Command to create a new personnel employee record.
/// </summary>
public sealed class CreateEmployeeCommand : IRequest<HrEmployeeDto>
{
    /// <summary>
    /// Gets or sets employee badge number.
    /// </summary>
    public string EmployeeNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets email.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets phone.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets social security number (AVS / CF).
    /// </summary>
    public string? SocialSecurityNumber { get; set; }

    /// <summary>
    /// Gets or sets department.
    /// </summary>
    public string? DepartmentName { get; set; }

    /// <summary>
    /// Gets or sets job title.
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Gets or sets employment status ("active", "onboarding").
    /// </summary>
    public string EmploymentStatus { get; set; } = "active";

    /// <summary>
    /// Gets or sets employment type ("full_time", "part_time").
    /// </summary>
    public string EmploymentType { get; set; } = "full_time";

    /// <summary>
    /// Gets or sets hire date.
    /// </summary>
    public DateTime HireDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets gross annual salary (RAL).
    /// </summary>
    public decimal BaseAnnualSalary { get; set; }

    /// <summary>
    /// Gets or sets hourly cost rate.
    /// </summary>
    public decimal HourlyCostRate { get; set; }

    /// <summary>
    /// Gets or sets currency ("CHF", "EUR").
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Gets or sets FTE workload ratio (e.g. 1.0).
    /// </summary>
    public decimal FteRatio { get; set; } = 1.0m;
}
