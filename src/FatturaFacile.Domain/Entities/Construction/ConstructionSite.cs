// <copyright file="ConstructionSite.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Construction;

/// <summary>
/// Represents a building site or civil engineering project (cantiere edile),
/// maintaining project codes, physical site location, designated site supervisors,
/// statutory safety coordinators (Coordinatore della Sicurezza), and overall contract budgets.
/// </summary>
public sealed class ConstructionSite : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private ConstructionSite()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization executing the construction contract.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique internal or public contract project identifier (e.g., "CANT-2026/01").
    /// </summary>
    public string ProjectCode { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the descriptive project or construction site name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the unique identifier of the commissioning client or contracting authority (committente).
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Gets the physical geographic and postal address of the active building site.
    /// </summary>
    public Address SiteAddress { get; private set; } = null!;

    /// <summary>
    /// Gets the full name of the appointed site director or project manager (Direttore di Cantiere).
    /// </summary>
    public string SiteManagerName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the name of the appointed workplace safety coordinator (Coordinatore della Sicurezza in fase di Esecuzione - CSE).
    /// </summary>
    public string? SafetyCoordinatorName { get; private set; }

    /// <summary>
    /// Gets the official commencement date of construction site operations.
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// Gets the anticipated completion deadline date according to the contractual work schedule (cronoprogramma).
    /// </summary>
    public DateTime? ProjectedEndDate { get; }

    /// <summary>
    /// Gets the total agreed contractual budget awarded for the construction project.
    /// </summary>
    public Money TotalContractBudget { get; private set; } = Money.Euro(0);

    /// <summary>
    /// Gets the operational lifecycle status of the construction site (e.g., "Active", "Suspended", "Completed").
    /// </summary>
    public string Status { get; } = "Active";

    /// <summary>
    /// Gets or sets the UTC timestamp when the construction site record was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier or username of the user who registered this construction site.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this construction site record was last updated.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier or username of the user who last updated this site record.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates a new construction site management record.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="projectCode">The unique site or contract code.</param>
    /// <param name="name">The descriptive project name.</param>
    /// <param name="clientId">The client principal or contracting authority identifier.</param>
    /// <param name="address">The physical location address of the building site.</param>
    /// <param name="manager">The appointed site manager (Direttore di Cantiere).</param>
    /// <param name="start">The site opening date.</param>
    /// <param name="budget">The monetary total contract budget.</param>
    /// <param name="safetyCoord">The optional safety coordinator name (CSE).</param>
    /// <returns>A newly initialized <see cref="ConstructionSite"/> aggregate root.</returns>
    public static ConstructionSite Create(
        Guid tenantId,
        string projectCode,
        string name,
        Guid clientId,
        Address address,
        string manager,
        DateTime start,
        Money budget,
        string? safetyCoord = null)
    {
        return new ConstructionSite
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectCode = projectCode?.Trim().ToUpperInvariant() ?? string.Empty,
            Name = name?.Trim() ?? string.Empty,
            ClientId = clientId,
            SiteAddress = address,
            SiteManagerName = manager?.Trim() ?? string.Empty,
            SafetyCoordinatorName = safetyCoord?.Trim() ?? string.Empty,
            StartDate = start,
            TotalContractBudget = budget,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
