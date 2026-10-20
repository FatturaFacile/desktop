using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Accounting;

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