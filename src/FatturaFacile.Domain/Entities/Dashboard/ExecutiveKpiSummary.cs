using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Dashboard;

/// <summary>
/// Represents executive kpi summary.
/// </summary>
public sealed class ExecutiveKpiSummary : Entity<Guid>, ITenantEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the period.
    /// </summary>
    public string Period { get; private set; } = string.Empty;

/// <summary>
    /// Gets the total revenue eur.
    /// </summary>
    public Money TotalRevenueEur { get; private set; } = Money.Euro(0);

/// <summary>
    /// Gets the total revenue chf.
    /// </summary>
    public Money TotalRevenueChf { get; private set; } = Money.Chf(0);

/// <summary>
    /// Gets the pending receivables eur.
    /// </summary>
    public Money PendingReceivablesEur { get; private set; } = Money.Euro(0);

/// <summary>
    /// Gets the pending receivables chf.
    /// </summary>
    public Money PendingReceivablesChf { get; private set; } = Money.Chf(0);

/// <summary>
    /// Gets the pending vat liability eur.
    /// </summary>
    public Money PendingVatLiabilityEur { get; private set; } = Money.Euro(0);

/// <summary>
    /// Gets the pending vat liability chf.
    /// </summary>
    public Money PendingVatLiabilityChf { get; private set; } = Money.Chf(0);

/// <summary>
    /// Gets the next tax deadline.
    /// </summary>
    public DateTime? NextTaxDeadline { get; private set; }

/// <summary>
    /// Gets the primary jurisdiction.
    /// </summary>
    public Jurisdiction PrimaryJurisdiction { get; private set; }

/// <summary>
    /// Gets the calculated at utc.
    /// </summary>
    public DateTime CalculatedAtUtc { get; private set; }

private ExecutiveKpiSummary()
    {
    }

/// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="period">The period.</param>
    /// <param name="revenueEur">The revenue eur.</param>
    /// <param name="revenueChf">The revenue chf.</param>
    /// <param name="receivablesEur">The receivables eur.</param>
    /// <param name="receivablesChf">The receivables chf.</param>
    /// <param name="vatEur">The vat eur.</param>
    /// <param name="vatChf">The vat chf.</param>
    /// <param name="nextTaxDeadline">The next tax deadline.</param>
    /// <param name="jurisdiction">The jurisdiction.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static ExecutiveKpiSummary Create(
        Guid tenantId,
        string period,
        Money revenueEur,
        Money revenueChf,
        Money receivablesEur,
        Money receivablesChf,
        Money vatEur,
        Money vatChf,
        DateTime? nextTaxDeadline,
        Jurisdiction jurisdiction)
    {
        return new ExecutiveKpiSummary
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Period = period,
            TotalRevenueEur = revenueEur,
            TotalRevenueChf = revenueChf,
            PendingReceivablesEur = receivablesEur,
            PendingReceivablesChf = receivablesChf,
            PendingVatLiabilityEur = vatEur,
            PendingVatLiabilityChf = vatChf,
            NextTaxDeadline = nextTaxDeadline,
            PrimaryJurisdiction = jurisdiction,
            CalculatedAtUtc = DateTime.UtcNow,
        };
    }
}
