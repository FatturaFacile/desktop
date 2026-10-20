using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Construction;

public sealed class ConstructionSite : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }

    /// <summary>
    /// CANT-2026/01
    /// </summary>
    public string ProjectCode { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Guid ClientId { get; private set; }
    public Address SiteAddress { get; private set; } = null!;
    public string SiteManagerName { get; private set; } = string.Empty;
    public string? SafetyCoordinatorName { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? ProjectedEndDate { get; }
    public Money TotalContractBudget { get; private set; } = Money.Euro(0);

    /// <summary>
    /// Active, Suspended, Completed
    /// </summary>
    public string Status { get; } = "Active";

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private ConstructionSite()
    {
    }

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