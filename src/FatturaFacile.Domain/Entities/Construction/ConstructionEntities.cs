using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
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

public sealed class BillOfQuantitiesItem : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid ConstructionSiteId { get; private set; }
    /// <summary>
    /// Regional tariff code (e.g. Lombardia or SIA/CRB/NPK)
    /// </summary>
    public string ItemCode { get; private set; } = string.Empty;
    public StandardType StandardType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string UnitOfMeasure { get; private set; } = "m2";
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Subtotal => decimal.Round(Quantity * UnitPrice, 2, MidpointRounding.AwayFromZero);

    private BillOfQuantitiesItem()
    {
    }

    public static BillOfQuantitiesItem Create(
        Guid tenantId,
        Guid siteId,
        string code,
        StandardType standard,
        string desc,
        string uom,
        decimal qty,
        decimal unitPrice)
    {
        return new BillOfQuantitiesItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ConstructionSiteId = siteId,
            ItemCode = code.Trim(),
            StandardType = standard,
            Description = desc.Trim(),
            UnitOfMeasure = uom.Trim(),
            Quantity = qty,
            UnitPrice = unitPrice,
        };
    }
}

public sealed class ProgressBillingSal : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public Guid ConstructionSiteId { get; private set; }
    /// <summary>
    /// SAL 1, SAL 2...
    /// </summary>
    public int SalNumber { get; private set; }
    public DateTime SalDate { get; private set; }
    /// <summary>
    /// 35.5%
    /// </summary>
    public decimal CumulativePercentage { get; private set; }
    public Money TotalCertifiedAmount { get; private set; } = Money.Euro(0);
    /// <summary>
    /// 5% garanzia
    /// </summary>
    public Money RetentionGuaranteeAmount { get; private set; } = Money.Euro(0);
    public bool IsInvoiced { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private ProgressBillingSal()
    {
    }

    public static ProgressBillingSal Create(
        Guid tenantId,
        Guid siteId,
        int number,
        DateTime date,
        decimal percent,
        Money certifiedAmount,
        Money retentionAmount)
    {
        return new ProgressBillingSal
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ConstructionSiteId = siteId,
            SalNumber = number,
            SalDate = date,
            CumulativePercentage = percent,
            TotalCertifiedAmount = certifiedAmount,
            RetentionGuaranteeAmount = retentionAmount,
            IsInvoiced = false,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}

public sealed class DeclarationOfConformity : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid ConstructionSiteId { get; private set; }
    public string CertificateNumber { get; private set; } = string.Empty;
    /// <summary>
    /// IT: DM 37/08; CH: NIV / OIBT
    /// </summary>
    public string StandardReference { get; private set; } = "DM 37/08";
    public string DeclarantEngineerName { get; private set; } = string.Empty;
    public DateTime IssueDate { get; private set; }
    /// <summary>
    /// Impianto elettrico, termoidraulico
    /// </summary>
    public string SystemTypeDescription { get; private set; } = string.Empty;

    private DeclarationOfConformity()
    {
    }

    public static DeclarationOfConformity Create(
        Guid tenantId,
        Guid siteId,
        string certNum,
        string standard,
        string engineer,
        DateTime issueDate,
        string systemType)
    {
        return new DeclarationOfConformity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ConstructionSiteId = siteId,
            CertificateNumber = certNum.Trim(),
            StandardReference = standard.Trim(),
            DeclarantEngineerName = engineer.Trim(),
            IssueDate = issueDate,
            SystemTypeDescription = systemType.Trim(),
        };
    }
}
