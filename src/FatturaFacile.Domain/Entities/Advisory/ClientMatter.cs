using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Advisory;

public sealed class ClientMatter : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }

    /// <summary>
    /// FASC-2026/042
    /// </summary>
    public string MatterNumber { get; private set; } = string.Empty;
    public Guid ClientId { get; private set; }
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Corporate, Litigation, Tax, Notary
    /// </summary>
    public string PracticeArea { get; private set; } = "TaxAdvisory";
    public bool KycAmlVerified { get; private set; }
    public RiskLevel AmlRiskLevel { get; private set; } = RiskLevel.Low;

    /// <summary>
    /// Open, InReview, Closed
    /// </summary>
    public string Status { get; } = "Open";

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private ClientMatter()
    {
    }

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

    public void VerifyKyc(RiskLevel riskLevel)
    {
        KycAmlVerified = true;
        AmlRiskLevel = riskLevel;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}