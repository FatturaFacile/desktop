using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Construction;

/// <summary>
/// Represents construction site.
/// </summary>
public sealed class ConstructionSite : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the cANT-2026/01.
    /// </summary>
    public string ProjectCode { get; private set; } = string.Empty;

/// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

/// <summary>
    /// Gets the client id.
    /// </summary>
    public Guid ClientId { get; private set; }

/// <summary>
    /// Gets the site address.
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
    /// Gets the start date.
    /// </summary>
    public DateTime StartDate { get; private set; }

/// <summary>
    /// Gets the projected end date.
    /// </summary>
    public DateTime? ProjectedEndDate { get; }

/// <summary>
    /// Gets the total contract budget.
    /// </summary>
    public Money TotalContractBudget { get; private set; } = Money.Euro(0);

/// <summary>
    /// Gets the active, Suspended, Completed.
    /// </summary>
    public string Status { get; } = "Active";

/// <summary>
    /// Gets or sets the created at utc.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

/// <summary>
    /// Gets or sets the created by.
    /// </summary>
    public string? CreatedBy { get; set; }

/// <summary>
    /// Gets or sets the updated at utc.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

/// <summary>
    /// Gets or sets the updated by.
    /// </summary>
    public string? UpdatedBy { get; set; }

private ConstructionSite()
    {
    }

/// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="projectCode">The project code.</param>
    /// <param name="name">The name.</param>
    /// <param name="clientId">The client id.</param>
    /// <param name="address">The address.</param>
    /// <param name="manager">The manager.</param>
    /// <param name="start">The start.</param>
    /// <param name="budget">The budget.</param>
    /// <param name="safetyCoord">The safety coord.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
            ProjectCode = projectCode.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            ClientId = clientId,
            SiteAddress = address,
            SiteManagerName = manager.Trim(),
            SafetyCoordinatorName = safetyCoord?.Trim(),
            StartDate = start,
            TotalContractBudget = budget,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
