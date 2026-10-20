using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Accounting;

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