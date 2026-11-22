// <copyright file="GeneralLedgerEntryDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Accounting;

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer object for a General Ledger voucher (Prima Nota).
/// </summary>
public sealed class GeneralLedgerEntryDto
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the mandatory owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the entry number (e.g. "GLE-2026-0001").
    /// </summary>
    public string EntryNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the accounting posting date.
    /// </summary>
    public DateTime PostingDate { get; set; }

    /// <summary>
    /// Gets or sets the document date.
    /// </summary>
    public DateTime DocumentDate { get; set; }

    /// <summary>
    /// Gets or sets the narrative entry description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source document classification.
    /// </summary>
    public string? SourceDocumentType { get; set; }

    /// <summary>
    /// Gets or sets the reference identifier.
    /// </summary>
    public Guid? ReferenceId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entry is locked.
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Gets or sets the ledger book of the entry.
    /// </summary>
    public LedgerBook LedgerType { get; set; } = LedgerBook.Statutory;

    /// <summary>
    /// Gets or sets the posting instant; null for a draft.
    /// </summary>
    public DateTime? PostedAt { get; set; }

    /// <summary>
    /// Gets or sets the entry this one reverses (storno), when it is a reversal.
    /// </summary>
    public Guid? ReversalOfEntryId { get; set; }

    /// <summary>
    /// Gets or sets the reason of the reversal.
    /// </summary>
    public string? ReversalReason { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a reversal of this entry exists.
    /// </summary>
    public bool IsReversed { get; set; }

    /// <summary>
    /// Gets a value indicating whether the entry is posted (locked).
    /// </summary>
    public bool IsPosted => IsLocked;

    /// <summary>
    /// Gets the entry date, the posting date.
    /// </summary>
    public DateTime Date => PostingDate;

    /// <summary>
    /// Gets or sets the total debit.
    /// </summary>
    public decimal TotalDebit { get; set; }

    /// <summary>
    /// Gets or sets the total credit amount of this entry.
    /// </summary>
    public decimal TotalCredit { get; set; }

    /// <summary>
    /// Gets or sets the list of lines comprising this voucher.
    /// </summary>
    public IList<GeneralLedgerLineDto> Lines { get; set; } = [];
}
