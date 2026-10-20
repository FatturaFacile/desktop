using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Accounting;

/// <summary>
/// Represents general ledger line.
/// </summary>
public sealed class GeneralLedgerLine : Entity<Guid>
{
    /// <summary>
    /// Gets the general ledger entry id.
    /// </summary>
    public Guid GeneralLedgerEntryId { get; }

/// <summary>
    /// Gets the account id.
    /// </summary>
    public Guid AccountId { get; }

/// <summary>
    /// Gets the debit amount.
    /// </summary>
    public decimal DebitAmount { get; }

/// <summary>
    /// Gets the credit amount.
    /// </summary>
    public decimal CreditAmount { get; }

/// <summary>
    /// Gets the line memo.
    /// </summary>
    public string? LineMemo { get; }

/// <summary>
    /// Initializes a new instance of the <see cref="GeneralLedgerLine"/> class.
    /// </summary>
    /// <param name="entryId">The entry id.</param>
    /// <param name="accountId">The account id.</param>
    /// <param name="debit">The debit.</param>
    /// <param name="credit">The credit.</param>
    /// <param name="memo">The memo.</param>
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
