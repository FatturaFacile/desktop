using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Construction;

/// <summary>
/// Represents progress billing sal.
/// </summary>
public sealed class ProgressBillingSal : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the construction site id.
    /// </summary>
    public Guid ConstructionSiteId { get; private set; }

/// <summary>
    /// Gets the sAL 1, SAL 2...
    /// </summary>
    public int SalNumber { get; private set; }

/// <summary>
    /// Gets the sal date.
    /// </summary>
    public DateTime SalDate { get; private set; }

/// <summary>
    /// Gets the 35.5%.
    /// </summary>
    public decimal CumulativePercentage { get; private set; }

/// <summary>
    /// Gets the total certified amount.
    /// </summary>
    public Money TotalCertifiedAmount { get; private set; } = Money.Euro(0);

/// <summary>
    /// Gets the 5% garanzia.
    /// </summary>
    public Money RetentionGuaranteeAmount { get; private set; } = Money.Euro(0);

/// <summary>
    /// Gets a value indicating whether invoiced.
    /// </summary>
    public bool IsInvoiced { get; private set; }

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

private ProgressBillingSal()
    {
    }

/// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="siteId">The site id.</param>
    /// <param name="number">The number.</param>
    /// <param name="date">The date.</param>
    /// <param name="percent">The percent.</param>
    /// <param name="certifiedAmount">The certified amount.</param>
    /// <param name="retentionAmount">The retention amount.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
