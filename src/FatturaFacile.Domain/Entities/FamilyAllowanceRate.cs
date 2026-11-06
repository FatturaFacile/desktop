// <copyright file="FamilyAllowanceRate.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Monthly family allowance amounts of a canton (child and education allowance), kept by validity date.
/// </summary>
public class FamilyAllowanceRate : IBaseEntity
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the canton code.
    /// </summary>
    public string Canton { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first day of validity.
    /// </summary>
    public DateTime ValidFrom { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the monthly child allowance (up to 16 years).
    /// </summary>
    public decimal ChildAllowance { get; set; }

    /// <summary>
    /// Gets or sets the monthly education allowance (15 to 25 years, in education).
    /// </summary>
    public decimal EducationAllowance { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
