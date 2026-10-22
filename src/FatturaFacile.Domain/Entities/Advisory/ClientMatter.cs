// <copyright file="ClientMatter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Advisory;

/// <summary>
/// Represents a legal, fiscal, or corporate consulting case (fascicolo/pratica) opened for a client,
/// tracking anti-money laundering (AML) compliance, practice area specialization, and matter status.
/// </summary>
public sealed class ClientMatter : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientMatter"/> class for entity framework or serialization.
    /// </summary>
    private ClientMatter()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization owning this client matter.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique matter reference code or file number (e.g., "FASC-2026/042").
    /// </summary>
    public string MatterNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the unique identifier of the client principal for whom this case is conducted.
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Gets the descriptive title or subject of the client matter or legal engagement.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the domain practice area specialization (e.g., Corporate, Litigation, TaxAdvisory, Notary).
    /// </summary>
    public string PracticeArea { get; private set; } = "TaxAdvisory";

    /// <summary>
    /// Gets a value indicating whether anti-money laundering (AML) and know-your-customer (KYC) identity verification checks have been formally completed.
    /// </summary>
    public bool KycAmlVerified { get; private set; }

    /// <summary>
    /// Gets the evaluated anti-money laundering risk level classified under applicable compliance regulations.
    /// </summary>
    public RiskLevel AmlRiskLevel { get; private set; } = RiskLevel.Low;

    /// <summary>
    /// Gets the current operational lifecycle status of the matter (e.g., Open, InReview, Closed).
    /// </summary>
    public string Status { get; } = "Open";

    /// <summary>
    /// Gets or sets the UTC timestamp when this client matter record was originally created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier or username of the user who created this matter record.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this client matter record was last modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier or username of the user who last modified this matter record.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates a new client matter engagement file.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="matterNumber">The case or matter tracking reference number.</param>
    /// <param name="clientId">The client principal unique identifier.</param>
    /// <param name="title">The subject or title of the engagement.</param>
    /// <param name="practiceArea">The advisory practice discipline (e.g., TaxAdvisory, Litigation).</param>
    /// <param name="kycVerified">Indicates whether initial KYC/AML identity verification is satisfied.</param>
    /// <param name="riskLevel">The statutory AML risk tier assigned to this client matter.</param>
    /// <returns>A newly initialized <see cref="ClientMatter"/> aggregate root.</returns>
    public static ClientMatter Create(
        Guid      tenantId,
        string    matterNumber,
        Guid      clientId,
        string    title,
        string    practiceArea,
        bool      kycVerified = false,
        RiskLevel riskLevel   = RiskLevel.Low)
    {
        return new ClientMatter
               {
                   Id             = Guid.NewGuid(),
                   TenantId       = tenantId,
                   MatterNumber   = matterNumber?.Trim().ToUpperInvariant() ?? string.Empty,
                   ClientId       = clientId,
                   Title          = title?.Trim()        ?? string.Empty,
                   PracticeArea   = practiceArea?.Trim() ?? string.Empty,
                   KycAmlVerified = kycVerified,
                   AmlRiskLevel   = riskLevel,
                   CreatedAtUtc   = DateTime.UtcNow,
               };
    }

    /// <summary>
    /// Records compliance confirmation of Know-Your-Customer (KYC) and Anti-Money Laundering (AML) due diligence,
    /// updating the statutory risk rating of the matter.
    /// </summary>
    /// <param name="riskLevel">The updated anti-money laundering risk classification level.</param>
    public void VerifyKyc(RiskLevel riskLevel)
    {
        KycAmlVerified = true;
        AmlRiskLevel   = riskLevel;
        UpdatedAtUtc   = DateTime.UtcNow;
    }

}
