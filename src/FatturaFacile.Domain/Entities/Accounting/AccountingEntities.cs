using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Accounting;

public sealed class GeneralLedgerAccount : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    /// <summary>
    /// Swiss KMU: &quot;1020&quot;, IT: &quot;01.01.001&quot;
    /// </summary>
    public string AccountCode { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public AccountCategory Category { get; private set; }
    public Jurisdiction Jurisdiction { get; private set; }
    public bool IsActive { get; private set; } = true;

    private GeneralLedgerAccount()
    {
    }

    public static GeneralLedgerAccount Create(
        Guid tenantId,
        string code,
        string name,
        AccountCategory category,
        Jurisdiction jurisdiction)
    {
        return new GeneralLedgerAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AccountCode = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Category = category,
            Jurisdiction = jurisdiction,
            IsActive = true,
        };
    }
}

public sealed class GeneralLedgerEntry : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    /// <summary>
    /// PNL-2026/001
    /// </summary>
    public string EntryNumber { get; private set; } = string.Empty;
    public DateTime PostingDate { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Currency Currency { get; private set; }
    public string? ReferenceDocument { get; private set; }

    private readonly List<GeneralLedgerLine> _lines = [];
    public IReadOnlyCollection<GeneralLedgerLine> Lines => _lines.AsReadOnly();

    public decimal TotalDebit => _lines.Sum(l => l.DebitAmount);
    public decimal TotalCredit => _lines.Sum(l => l.CreditAmount);
    public bool IsBalanced => Math.Abs(TotalDebit - TotalCredit) < 0.0001m;

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private GeneralLedgerEntry()
    {
    }

    public static GeneralLedgerEntry Create(
        Guid tenantId,
        string entryNumber,
        DateTime postingDate,
        string description,
        Currency currency,
        string? referenceDoc = null)
    {
        return new GeneralLedgerEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EntryNumber = entryNumber.Trim(),
            PostingDate = postingDate,
            Description = description.Trim(),
            Currency = currency,
            ReferenceDocument = referenceDoc,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void AddLine(Guid accountId, decimal debit, decimal credit, string? lineMemo = null) => _lines.Add(new GeneralLedgerLine(Id, accountId, debit, credit, lineMemo));
}

public sealed class GeneralLedgerLine : Entity<Guid>
{
    public Guid GeneralLedgerEntryId { get; }
    public Guid AccountId { get; }
    public decimal DebitAmount { get; }
    public decimal CreditAmount { get; }
    public string? LineMemo { get; }

    internal GeneralLedgerLine(Guid entryId, Guid accountId, decimal debit, decimal credit, string? memo)
    {
        Id = Guid.NewGuid();
        GeneralLedgerEntryId = entryId;
        AccountId = accountId;
        DebitAmount = debit;
        CreditAmount = credit;
        LineMemo = memo;
    }

    private GeneralLedgerLine()
    {
    }
}

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
