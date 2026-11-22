// <copyright file="Project.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A project (commessa) of the portfolio kept offline, with its rolled-up planned and actual figures (Mod 078). A template (#186) is a project with <see cref="IsTemplate"/> set.
/// </summary>
public class Project : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the legal entity the project belongs to.
    /// </summary>
    public Guid? LegalEntityId { get; set; }

    /// <summary>
    /// Gets or sets the unique project code.
    /// </summary>
    public string ProjectCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer identifier.
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the project manager identifier.
    /// </summary>
    public Guid? ProjectManagerId { get; set; }

    /// <summary>
    /// Gets or sets the project type.
    /// </summary>
    public ProjectType ProjectType { get; set; }

    /// <summary>
    /// Gets or sets the billing model.
    /// </summary>
    public ProjectBillingModel BillingModel { get; set; }

    /// <summary>
    /// Gets or sets the cost center identifier.
    /// </summary>
    public Guid? CostCenterId { get; set; }

    /// <summary>
    /// Gets or sets the planned start date.
    /// </summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the target end date.
    /// </summary>
    public DateOnly? TargetEndDate { get; set; }

    /// <summary>
    /// Gets or sets the lifecycle status.
    /// </summary>
    public ProjectStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the Italian CUP code (public investment project code).
    /// </summary>
    public string? CupCode { get; set; }

    /// <summary>
    /// Gets or sets the Italian CIG code (tender code).
    /// </summary>
    public string? CigCode { get; set; }

    /// <summary>
    /// Gets or sets the planned hours rolled up from the WBS.
    /// </summary>
    public decimal PlannedHours { get; set; }

    /// <summary>
    /// Gets or sets the actual hours rolled up from the WBS.
    /// </summary>
    public decimal ActualHours { get; set; }

    /// <summary>
    /// Gets or sets the planned cost rolled up from the WBS.
    /// </summary>
    public decimal PlannedCost { get; set; }

    /// <summary>
    /// Gets or sets the actual cost rolled up from the WBS.
    /// </summary>
    public decimal ActualCost { get; set; }

    /// <summary>
    /// Gets or sets the planned revenue rolled up from the WBS.
    /// </summary>
    public decimal PlannedRevenue { get; set; }

    /// <summary>
    /// Gets or sets the percent complete (0-100) rolled up from the WBS.
    /// </summary>
    public decimal PercentComplete { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the project is a reusable template (#186).
    /// </summary>
    public bool IsTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template this project was instantiated from.
    /// </summary>
    public Guid? TemplateSourceId { get; set; }

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
