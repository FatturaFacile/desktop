// <copyright file="GeneralLedgerEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents a double-entry journal voucher in the General Ledger (Prima Nota).
/// Mandatory tenant-scoped financial document.
/// </summary>
public class GeneralLedgerEntry : ITenantScopedEntity
{

    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the mandatory owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the sequential entry/voucher number (e.g. "GLE-2026-0001").
    /// </summary>
    public string EntryNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the accounting posting date.
    /// </summary>
    public DateTime PostingDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the original document date.
    /// </summary>
    public DateTime DocumentDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the narrative entry description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source document classification ("invoice", "bank_statement", "manual", "payroll").
    /// </summary>
    public string? SourceDocumentType { get; set; }

    /// <summary>
    /// Gets or sets the foreign key identifier of the originating source document.
    /// </summary>
    public Guid? ReferenceId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entry is locked against modification.
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the soft delete timestamp.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the debit/credit lines comprising this balanced voucher.
    /// </summary>
    public ICollection<GeneralLedgerLine> Lines { get; set; } = [];
}
