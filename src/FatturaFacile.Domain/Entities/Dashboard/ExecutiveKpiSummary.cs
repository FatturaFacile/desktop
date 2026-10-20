using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Dashboard;

public sealed class ExecutiveKpiSummary : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Period { get; private set; } = string.Empty;
    public Money TotalRevenueEur { get; private set; } = Money.Euro(0);
    public Money TotalRevenueChf { get; private set; } = Money.Chf(0);
    public Money PendingReceivablesEur { get; private set; } = Money.Euro(0);
    public Money PendingReceivablesChf { get; private set; } = Money.Chf(0);
    public Money PendingVatLiabilityEur { get; private set; } = Money.Euro(0);
    public Money PendingVatLiabilityChf { get; private set; } = Money.Chf(0);
    public DateTime? NextTaxDeadline { get; private set; }
    public Jurisdiction PrimaryJurisdiction { get; private set; }
    public DateTime CalculatedAtUtc { get; private set; }

    private ExecutiveKpiSummary()
    {
    }

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