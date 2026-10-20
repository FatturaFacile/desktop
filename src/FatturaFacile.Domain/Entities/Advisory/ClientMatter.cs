using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Advisory;

/// <summary>
/// Represents client matter.
/// </summary>
public sealed class ClientMatter : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the fASC-2026/042.
    /// </summary>
    public string MatterNumber { get; private set; } = string.Empty;

/// <summary>
    /// Gets the client id.
    /// </summary>
    public Guid ClientId { get; private set; }

/// <summary>
    /// Gets the title.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

/// <summary>
    /// Gets the corporate, Litigation, Tax, Notary.
    /// </summary>
    public string PracticeArea { get; private set; } = "TaxAdvisory";

/// <summary>
    /// Gets a value indicating whether kyc aml verified.
    /// </summary>
    public bool KycAmlVerified { get; private set; }

/// <summary>
    /// Gets the aml risk level.
    /// </summary>
    public RiskLevel AmlRiskLevel { get; private set; } = RiskLevel.Low;

/// <summary>
    /// Gets the open, InReview, Closed.
    /// </summary>
    public string Status { get; } = "Open";

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

private ClientMatter()
    {
    }

/// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="matterNumber">The matter number.</param>
    /// <param name="clientId">The client id.</param>
    /// <param name="title">The title.</param>
    /// <param name="practiceArea">The practice area.</param>
    /// <param name="kycVerified">The kyc verified.</param>
    /// <param name="riskLevel">The risk level.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static ClientMatter Create(
        Guid tenantId,
        string matterNumber,
        Guid clientId,
        string title,
        string practiceArea,
        bool kycVerified = false,
        RiskLevel riskLevel = RiskLevel.Low)
    {
        return new ClientMatter
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MatterNumber = matterNumber.Trim().ToUpperInvariant(),
            ClientId = clientId,
            Title = title.Trim(),
            PracticeArea = practiceArea.Trim(),
            KycAmlVerified = kycVerified,
            AmlRiskLevel = riskLevel,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

/// <summary>
    /// Verify Kyc.
    /// </summary>
    /// <param name="riskLevel">The risk level.</param>
    public void VerifyKyc(RiskLevel riskLevel)
    {
        KycAmlVerified = true;
        AmlRiskLevel = riskLevel;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
