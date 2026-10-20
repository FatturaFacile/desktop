using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Accounting;

public sealed class VatSettlementPeriod : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public int Year { get; private set; }

    /// <summary>
    /// 1..12 for monthly or 1..4 for quarterly
    /// </summary>
    public int PeriodNumber { get; private set; }
    public VatSettlementPeriodType PeriodType { get; private set; }
    public Jurisdiction Jurisdiction { get; private set; }

    public decimal TotalOutputVat { get; private set; }
    public decimal TotalInputVat { get; private set; }
    public decimal PriorCreditCarriedForward { get; private set; }
    public decimal NetVatPayable => Math.Max(0m, TotalOutputVat - TotalInputVat - PriorCreditCarriedForward);
    public decimal NetVatCredit => Math.Max(0m, TotalInputVat + PriorCreditCarriedForward - TotalOutputVat);
    public bool IsFinalized { get; private set; }
    public DateTime? FinalizedAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private VatSettlementPeriod()
    {
    }

    public static VatSettlementPeriod Create(
        Guid tenantId,
        int year,
        int periodNumber,
        VatSettlementPeriodType type,
        Jurisdiction jurisdiction,
        decimal outputVat,
        decimal inputVat,
        decimal priorCredit = 0m)
    {
        return new VatSettlementPeriod
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Year = year,
            PeriodNumber = periodNumber,
            PeriodType = type,
            Jurisdiction = jurisdiction,
            TotalOutputVat = outputVat,
            TotalInputVat = inputVat,
            PriorCreditCarriedForward = priorCredit,
            IsFinalized = false,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void FinalizeSettlement()
    {
        IsFinalized = true;
        FinalizedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}