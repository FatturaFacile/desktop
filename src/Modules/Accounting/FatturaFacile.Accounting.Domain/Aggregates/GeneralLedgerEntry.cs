// <copyright file="GeneralLedgerEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Primitives;

namespace FatturaFacile.Accounting.Domain.Aggregates;

/// <summary>
/// Represents a double-entry general ledger journal entry aggregate root.
/// </summary>
public sealed class GeneralLedgerEntry : AggregateRoot<Guid>, IHasTenant, ISoftDeletable, IAuditableEntity
{
    private readonly List<GeneralLedgerLine> _lines = [];

    private GeneralLedgerEntry() { }

    /// <inheritdoc />
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Gets the unique sequential journal entry document number (e.g., "JE-2026-0001").
    /// </summary>
    public string EntryNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the accounting transaction posting date.
    /// </summary>
    public DateOnly Date { get; private set; }

    /// <summary>
    /// Gets the general explanation or justification for this journal entry.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the optional identifier of an originating source document (e.g., invoice, receipt).
    /// </summary>
    public Guid? ReferenceDocumentId { get; private set; }

    /// <summary>
    /// Gets the optional source document type name (e.g., "Invoice", "Payment").
    /// </summary>
    public string? ReferenceDocumentType { get; private set; }

    /// <summary>
    /// Gets the cumulative total of all debit lines in this entry.
    /// </summary>
    public decimal TotalDebit { get; private set; }

    /// <summary>
    /// Gets the cumulative total of all credit lines in this entry.
    /// </summary>
    public decimal TotalCredit { get; private set; }

    /// <inheritdoc />
    public bool IsDeleted { get; }

    /// <inheritdoc />
    public DateTimeOffset? DeletedAt { get;  }

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; private set; }

    /// <inheritdoc />
    public string? CreatedBy { get; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAt { get;  }

    /// <inheritdoc />
    public string? UpdatedBy { get; }

    /// <summary>
    /// Gets the read-only collection of debit and credit lines comprising this journal entry.
    /// </summary>
    public IReadOnlyCollection<GeneralLedgerLine> Lines => _lines.AsReadOnly();

    /// <summary>
    /// Factory method to create a new <see cref="GeneralLedgerEntry"/> aggregate root.
    /// </summary>
    /// <param name="tenantId">The owning tenant identifier.</param>
    /// <param name="entryNumber">The unique entry reference number.</param>
    /// <param name="date">The accounting posting date.</param>
    /// <param name="description">The transaction description.</param>
    /// <param name="refDocId">The optional originating document identifier.</param>
    /// <param name="refDocType">The optional originating document type.</param>
    /// <returns>A new <see cref="GeneralLedgerEntry"/> instance.</returns>
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
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Adds a balanced debit or credit line to the journal entry and updates running totals.
    /// </summary>
    /// <param name="accountId">The charted account identifier.</param>
    /// <param name="accountCode">The charted account code.</param>
    /// <param name="description">The line description.</param>
    /// <param name="type">The line entry type (Debit or Credit).</param>
    /// <param name="amount">The line amount.</param>
    public void AddLine(Guid accountId, string accountCode, string description, EntryType type, decimal amount)
    {
        var line = GeneralLedgerLine.Create(TenantId, accountId, accountCode, description, type, amount);
        _lines.Add(line);

        TotalDebit = _lines.Where(l => l.Type == EntryType.Debit).Sum(l => l.Amount);
        TotalCredit = _lines.Where(l => l.Type == EntryType.Credit).Sum(l => l.Amount);
    }

    /// <summary>
    /// Evaluates whether total debits equal total credits, fulfilling fundamental double-entry balance requirements.
    /// </summary>
    /// <returns><see langword="true"/> if the journal entry is balanced; otherwise, <see langword="false"/>.</returns>
    public bool IsBalanced() => TotalDebit == TotalCredit;
}
