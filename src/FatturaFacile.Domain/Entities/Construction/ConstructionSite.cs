// <copyright file="ConstructionSite.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Construction;

/// <summary>
/// Represents a building site or civil engineering project.
/// </summary>
public sealed class ConstructionSite : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConstructionSite"/> class.
    /// Required by Entity Framework.
    /// </summary>
    private ConstructionSite()
    {
    }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the project code.
    /// </summary>
    public string ProjectCode { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the project name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the customer identifier.
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Gets the construction site address.
    /// </summary>
    public Address SiteAddress { get; private set; } = null!;

    /// <summary>
    /// Gets the site manager name.
    /// </summary>
    public string SiteManagerName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the safety coordinator name.
    /// </summary>
    public string? SafetyCoordinatorName { get; private set; }

    /// <summary>
    /// Gets the project start date.
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// Gets the projected completion date.
    /// </summary>
    public DateTime? ProjectedEndDate { get; private set; }

    /// <summary>
    /// Gets the total contract budget.
    /// </summary>
    public Money TotalContractBudget { get; private set; } = Money.Euro(0);

    /// <summary>
    /// Gets the current construction site status.
    /// </summary>
    public string Status { get; private set; } =        ConstructionSiteStatus.Active;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the creator identifier.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the last updater identifier.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates a new construction site.
    /// </summary>
    /// <param name="tenantId">Tenant identifier.</param>
    /// <param name="data">Construction site data.</param>
    /// <returns>A configured construction site.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when data is null.
    /// </exception>
    public static ConstructionSite Create(
        Guid tenantId,
        ConstructionSiteData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return new ConstructionSite
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectCode = data.ProjectCode.Trim().ToUpperInvariant(),
            Name = data.Name.Trim(),
            ClientId = data.ClientId,
            SiteAddress = data.SiteAddress,
            SiteManagerName = data.SiteManagerName.Trim(),
            SafetyCoordinatorName = data.SafetyCoordinatorName?.Trim(),
            StartDate = data.StartDate,
            TotalContractBudget = data.TotalContractBudget,
            Status = ConstructionSiteStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Suspends the construction site.
    /// </summary>
    public void Suspend() => Status = ConstructionSiteStatus.Suspended;

    /// <summary>
    /// Completes the construction site.
    /// </summary>
    public void Complete() => Status = ConstructionSiteStatus.Completed;

    /// <summary>
    /// Updates the projected completion date.
    /// </summary>
    /// <param name="projectedEndDate">
    /// New projected completion date.
    /// </param>
    public void SetProjectedEndDate(DateTime projectedEndDate) => ProjectedEndDate = projectedEndDate;
}
