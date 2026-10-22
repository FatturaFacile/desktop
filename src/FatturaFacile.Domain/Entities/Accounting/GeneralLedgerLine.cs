// <copyright file="GeneralLedgerLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Accounting;

/// <summary>
/// Represents an individual debit or credit line within a double-entry general ledger journal entry,
/// linking an account to a monetary value movement.
/// </summary>
public sealed class GeneralLedgerLine : Entity<Guid>
{

    /// <summary>
    /// Initializes a new instance of the <see cref="GeneralLedgerLine"/> class with entry reference,
    /// account identifier, debit/credit values, and optional memo.
    /// </summary>
    /// <param name="entryId">The unique identifier of the parent <see cref="GeneralLedgerEntry"/>.</param>
    /// <param name="accountId">The unique identifier of the affected <see cref="GeneralLedgerAccount"/>.</param>
    /// <param name="debit">The debit amount (0 if recording a credit movement).</param>
    /// <param name="credit">The credit amount (0 if recording a debit movement).</param>
    /// <param name="memo">An optional line-level description or transaction detail.</param>
    internal GeneralLedgerLine(
        Guid    entryId,
        Guid    accountId,
        decimal debit,
        decimal credit,
        string? memo)
    {
        Id                   = Guid.NewGuid();
        GeneralLedgerEntryId = entryId;
        AccountId            = accountId;
        DebitAmount          = debit;
        CreditAmount         = credit;
        LineMemo             = memo;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeneralLedgerLine"/> class for entity framework or serialization.
    /// </summary>
    private GeneralLedgerLine()
    {
    }

    /// <summary>
    /// Gets the unique identifier of the parent journal entry (<see cref="GeneralLedgerEntry"/>) to which this line belongs.
    /// </summary>
    public Guid GeneralLedgerEntryId { get; }

    /// <summary>
    /// Gets the unique identifier of the target <see cref="GeneralLedgerAccount"/> debited or credited.
    /// </summary>
    public Guid AccountId { get; }

    /// <summary>
    /// Gets the monetary amount debited to the ledger account (0.0 if this line represents a credit).
    /// </summary>
    public decimal DebitAmount { get; }

    /// <summary>
    /// Gets the monetary amount credited to the ledger account (0.0 if this line represents a debit).
    /// </summary>
    public decimal CreditAmount { get; }

    /// <summary>
    /// Gets an optional explanatory note or memo providing context for this specific line item.
    /// </summary>
    public string? LineMemo { get; }

}
