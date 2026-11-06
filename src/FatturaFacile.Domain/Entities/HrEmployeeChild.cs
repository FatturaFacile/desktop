// <copyright file="HrEmployeeChild.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A child of an employee, for family allowances (Kinder- und Ausbildungszulagen) and the withholding tax tariff.
/// </summary>
public class HrEmployeeChild : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the company (tenant) identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the employee identifier.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date of birth.
    /// </summary>
    public DateTime DateOfBirth { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets a value indicating whether the child is in post-compulsory education (education allowance from 15 to 25).
    /// </summary>
    public bool InEducation { get; set; }

    /// <summary>
    /// Gets or sets the expected end of education.
    /// </summary>
    public DateTime? EducationEndDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the allowance is paid through this employer.
    /// </summary>
    public bool AllowanceEligible { get; set; } = true;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
