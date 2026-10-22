// <copyright file="GeneralLedgerEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Accounting;

/// <summary>
/// Represents a double-entry journal entry aggregate (prima nota / Buchungssatz) in the general ledger,
/// enforcing the fundamental accounting equation where total debits must equal total credits.
/// </summary>
public sealed class GeneralLedgerEntry : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{

    /// <summary>
    /// The internal collection of debit and credit lines comprising this journal entry.
    /// </summary>
    private readonly List<GeneralLedgerLine> lines = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GeneralLedgerEntry"/> class.
    /// </summary>
    private GeneralLedgerEntry()
    {
    }

    /// <summary>
    /// Gets or sets the unique tenant identifier for multi-tenant data partitioning.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique progressive journal voucher entry number (e.g., "PNL-2026/001" or "JRN-2026-0042").
    /// </summary>
    public string EntryNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the official accounting posting date for the journal transaction.
    /// </summary>
    public DateTime PostingDate { get; private set; }

    /// <summary>
    /// Gets the general business rationale or narrative description of the accounting transaction.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the base transaction currency of this journal voucher.
    /// </summary>
    public Currency Currency { get; private set; }

    /// <summary>
    /// Gets an optional external or source document reference (such as an invoice number, receipt ID, or bank reference).
    /// </summary>
    public string? ReferenceDocument { get; private set; }

    /// <summary>
    /// Gets the read-only collection of debit and credit lines comprising this journal entry.
    /// </summary>
    public IReadOnlyCollection<GeneralLedgerLine> Lines => lines.AsReadOnly();

    /// <summary>
    /// Gets the aggregate debit amount summed across all debit lines in this entry.
    /// </summary>
    public decimal TotalDebit => lines.Sum(selector: l => l.DebitAmount);

    /// <summary>
    /// Gets the aggregate credit amount summed across all credit lines in this entry.
    /// </summary>
    public decimal TotalCredit => lines.Sum(selector: l => l.CreditAmount);

    /// <summary>
    /// Gets a value indicating whether the entry satisfies the double-entry balance invariant (<c>|TotalDebit - TotalCredit| &lt; 0.0001</c>).
    /// </summary>
    public bool IsBalanced => Math.Abs(value: TotalDebit - TotalCredit) < 0.0001m;

    /// <summary>
    /// Gets or sets the UTC timestamp when the journal entry was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the user identifier or system process that authored this journal entry.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this journal entry was last modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the user identifier or system process that last updated this entry.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates a new <see cref="GeneralLedgerEntry"/> aggregate ready to receive journal lines.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="entryNumber">The unique journal entry voucher identifier (e.g., "PNL-2026/001").</param>
    /// <param name="postingDate">The accounting posting date.</param>
    /// <param name="description">The business narrative for the entry.</param>
    /// <param name="currency">The accounting currency.</param>
    /// <param name="referenceDoc">An optional source document reference.</param>
    /// <returns>A newly instantiated <see cref="GeneralLedgerEntry"/> aggregate.</returns>
    public static GeneralLedgerEntry Create(
        Guid     tenantId,
        string   entryNumber,
        DateTime postingDate,
        string   description,
        Currency currency,
        string?  referenceDoc = null)
    {
        return new GeneralLedgerEntry
               {
                   Id                = Guid.NewGuid(),
                   TenantId          = tenantId,
                   EntryNumber       = entryNumber?.Trim() ?? string.Empty,
                   PostingDate       = postingDate,
                   Description       = description?.Trim() ?? string.Empty,
                   Currency          = currency,
                   ReferenceDocument = referenceDoc,
                   CreatedAtUtc      = DateTime.UtcNow,
               };
    }

    /// <summary>
    /// Appends a new debit or credit line to the journal entry.
    /// </summary>
    /// <param name="accountId">The unique identifier of the target <see cref="GeneralLedgerAccount"/>.</param>
    /// <param name="debit">The positive debit amount to record (or 0 if this is a credit line).</param>
    /// <param name="credit">The positive credit amount to record (or 0 if this is a debit line).</param>
    /// <param name="lineMemo">An optional explanatory note specific to this line item.</param>
    public void AddLine(
        Guid    accountId,
        decimal debit,
        decimal credit,
        string? lineMemo = null) =>
        lines.Add(
                  item: new GeneralLedgerLine(
                                              entryId: Id,
                                              accountId: accountId,
                                              debit: debit,
                                              credit: credit,
                                              memo: lineMemo));

}
