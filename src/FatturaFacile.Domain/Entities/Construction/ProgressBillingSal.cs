// <copyright file="ProgressBillingSal.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Construction;

/// <summary>
/// Represents a construction interim progress billing certificate or work advancement state
/// (SAL - Stato Avanzamento Lavori / Situazione Lavori), certifying executed quantities,
/// cumulative progress percentages, and contractual warranty retention deductions (Ritenuta a garanzia).
/// </summary>
public sealed class ProgressBillingSal : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private ProgressBillingSal()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization executing the construction project.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique identifier of the construction site to which this progress billing belongs.
    /// </summary>
    public Guid ConstructionSiteId { get; private set; }

    /// <summary>
    /// Gets the sequential certificate iteration number (e.g., 1 for SAL 1, 2 for SAL 2).
    /// </summary>
    public int SalNumber { get; private set; }

    /// <summary>
    /// Gets the certified progress cutoff date on which site accounting and quantity survey took place.
    /// </summary>
    public DateTime SalDate { get; private set; }

    /// <summary>
    /// Gets the cumulative percentage of total contract works completed up to this SAL iteration (e.g., 35.5%).
    /// </summary>
    public decimal CumulativePercentage { get; private set; }

    /// <summary>
    /// Gets the cumulative certified monetary value of works completed and approved for payment by the site supervisor (Direttore dei Lavori).
    /// </summary>
    public Money TotalCertifiedAmount { get; private set; } = Money.Euro(0);

    /// <summary>
    /// Gets the contractual defect liability retention amount withheld from payment (e.g., 5% ritenuta a garanzia until final testing/collaudo).
    /// </summary>
    public Money RetentionGuaranteeAmount { get; private set; } = Money.Euro(0);

    /// <summary>
    /// Gets a value indicating whether an interim progress payment invoice has been issued based on this certificate.
    /// </summary>
    public bool IsInvoiced { get; private set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this progress certificate was generated.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier or username of the user who recorded this progress billing certificate.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this progress certificate was last updated.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier or username of the user who last updated this certificate.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates a new interim work progress certificate (SAL) for a construction site.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="siteId">The construction site identifier.</param>
    /// <param name="number">The sequential SAL index number.</param>
    /// <param name="date">The survey and certification date.</param>
    /// <param name="percent">The cumulative completion percentage of the contract.</param>
    /// <param name="certifiedAmount">The gross cumulative monetary value of works approved.</param>
    /// <param name="retentionAmount">The statutory or contractual performance warranty retention amount withheld.</param>
    /// <returns>A new <see cref="ProgressBillingSal"/> aggregate root initialized in un-invoiced status.</returns>
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
