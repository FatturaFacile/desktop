using FatturaFacile.Domain.Shared.Primitives;

namespace FatturaFacile.Accounting.Domain.Aggregates;

public enum EntryType
{
    Debit = 1,
    Credit = 2
}

public sealed class GeneralLedgerLine : Entity<Guid>, IHasTenant
{
    public Guid TenantId { get; private set; }
    public Guid GeneralLedgerEntryId { get; private set; }
    public Guid AccountId { get; private set; }
    public string AccountCode { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public EntryType Type { get; private set; }
    public decimal Amount { get; private set; }

    private GeneralLedgerLine() { }

    public static GeneralLedgerLine Create(
        Guid tenantId,
        Guid accountId,
        string accountCode,
        string description,
        EntryType type,
        decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive.", nameof(amount));

        return new GeneralLedgerLine
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AccountId = accountId,
            AccountCode = accountCode,
            Description = description,
            Type = type,
            Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero)
        };
    }
}

public sealed class GeneralLedgerEntry : AggregateRoot<Guid>, IHasTenant, ISoftDeletable, IAuditableEntity
{
    private readonly List<GeneralLedgerLine> _lines = [];

    public Guid TenantId { get; private set; }
    public string EntryNumber { get; private set; } = string.Empty;
    public DateOnly Date { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Guid? ReferenceDocumentId { get; private set; }
    public string? ReferenceDocumentType { get; private set; }

    public decimal TotalDebit { get; private set; }
    public decimal TotalCredit { get; private set; }

    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    // Audit
    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    public IReadOnlyCollection<GeneralLedgerLine> Lines => _lines.AsReadOnly();

    private GeneralLedgerEntry() { }

    public static GeneralLedgerEntry Create(
        Guid tenantId,
        string entryNumber,
        DateOnly date,
        string description,
        Guid? refDocId = null,
        string? refDocType = null)
    {
        return new GeneralLedgerEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EntryNumber = entryNumber,
            Date = date,
            Description = description,
            ReferenceDocumentId = refDocId,
            ReferenceDocumentType = refDocType,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void AddLine(Guid accountId, string accountCode, string description, EntryType type, decimal amount)
    {
        var line = GeneralLedgerLine.Create(TenantId, accountId, accountCode, description, type, amount);
        _lines.Add(line);

        TotalDebit = _lines.Where(l => l.Type == EntryType.Debit).Sum(l => l.Amount);
        TotalCredit = _lines.Where(l => l.Type == EntryType.Credit).Sum(l => l.Amount);
    }

    public bool IsBalanced() => TotalDebit == TotalCredit;
}
