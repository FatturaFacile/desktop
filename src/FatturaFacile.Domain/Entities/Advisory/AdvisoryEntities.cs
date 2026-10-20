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

public sealed class BillableTimeEntry : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid ClientMatterId { get; private set; }
    public Guid ProfessionalUserId { get; private set; }
    public DateTime ActivityDate { get; private set; }
    public int MinutesSpent { get; private set; }
    public decimal HourlyRate { get; private set; }
    public Currency Currency { get; private set; } = Currency.EUR;
    public string Description { get; private set; } = string.Empty;
    public bool IsInvoiced { get; private set; }

    public decimal TotalFee => decimal.Round(MinutesSpent / 60m * HourlyRate, 2, MidpointRounding.AwayFromZero);

    private BillableTimeEntry()
    {
    }

    public static BillableTimeEntry Create(
        Guid tenantId,
        Guid matterId,
        Guid userId,
        DateTime date,
        int minutes,
        decimal rate,
        Currency currency,
        string description)
    {
        return new BillableTimeEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ClientMatterId = matterId,
            ProfessionalUserId = userId,
            ActivityDate = date,
            MinutesSpent = minutes,
            HourlyRate = rate,
            Currency = currency,
            Description = description.Trim(),
            IsInvoiced = false,
        };
    }

    public void MarkInvoiced() => IsInvoiced = true;
}

public sealed class ProfessionalFeeNote : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    /// <summary>
    /// PARC-2026/01
    /// </summary>
    public string DocumentNumber { get; private set; } = string.Empty;
    public Guid ClientMatterId { get; private set; }
    public DateTime DocumentDate { get; private set; }
    public Currency Currency { get; private set; } = Currency.EUR;

    /// <summary>
    /// Fee calculations
    /// </summary>
    public decimal GrossProfessionalFees { get; private set; }
    public decimal ExpensesNonTaxable { get; private set; }

    /// <summary>
    /// Italian specific statutory contributions
    /// </summary>
    public decimal ItCassaPrevidenzialePercentage { get; private set; } = 4.0m; // 4% Cassa Forense/Dottori Commercialisti
    public decimal ItCassaPrevidenzialeAmount { get; private set; }
    /// <summary>
    /// 20% Ritenuta d&#39;acconto
    /// </summary>
    public decimal ItRitenutaAccontoPercentage { get; private set; } = 20.0m;
    public decimal ItRitenutaAccontoAmount { get; private set; }

    /// <summary>
    /// Swiss specific fiduciary withholding
    /// </summary>
    public decimal ChFiduciaryTaxPercentage { get; }
    public decimal ChFiduciaryTaxAmount { get; }

    public decimal TotalVat { get; private set; }
    public decimal NetPayable { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private ProfessionalFeeNote()
    {
    }

    public static ProfessionalFeeNote CreateItalian(
        Guid tenantId,
        string docNumber,
        Guid matterId,
        DateTime date,
        decimal grossFees,
        decimal nonTaxableExpenses,
        decimal vatRate = 22.0m,
        decimal cassaPercent = 4.0m,
        decimal ritenutaPercent = 20.0m)
    {
        var cassaAmt = decimal.Round(grossFees * (cassaPercent / 100m), 2, MidpointRounding.AwayFromZero);
        var vatBase = grossFees + cassaAmt;
        var vatAmt = decimal.Round(vatBase * (vatRate / 100m), 2, MidpointRounding.AwayFromZero);
        var ritenutaAmt = decimal.Round(grossFees * (ritenutaPercent / 100m), 2, MidpointRounding.AwayFromZero);
        var netPayable = grossFees + cassaAmt + vatAmt + nonTaxableExpenses - ritenutaAmt;

        return new ProfessionalFeeNote
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DocumentNumber = docNumber.Trim().ToUpperInvariant(),
            ClientMatterId = matterId,
            DocumentDate = date,
            Currency = Currency.EUR,
            GrossProfessionalFees = grossFees,
            ExpensesNonTaxable = nonTaxableExpenses,
            ItCassaPrevidenzialePercentage = cassaPercent,
            ItCassaPrevidenzialeAmount = cassaAmt,
            ItRitenutaAccontoPercentage = ritenutaPercent,
            ItRitenutaAccontoAmount = ritenutaAmt,
            TotalVat = vatAmt,
            NetPayable = netPayable,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
