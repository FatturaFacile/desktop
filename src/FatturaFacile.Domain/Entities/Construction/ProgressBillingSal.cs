using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Construction;

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