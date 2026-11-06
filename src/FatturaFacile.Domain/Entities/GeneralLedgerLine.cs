// <copyright file="GeneralLedgerLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents an individual debit or credit line on a General Ledger entry voucher.
/// </summary>
public class GeneralLedgerLine : IBaseEntity
{

    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the foreign key identifier of the parent entry.
    /// </summary>
    public Guid GeneralLedgerEntryId { get; set; }

    /// <summary>
    /// Gets or sets the foreign key identifier of the affected account.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Gets or sets the 1-based sequential line number.
    /// </summary>
    public int LineNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the debit amount (Dare).
    /// </summary>
    public decimal Debit { get; set; }

    /// <summary>
    /// Gets or sets the credit amount (Avere).
    /// </summary>
    public decimal Credit { get; set; }

    /// <summary>
    /// Gets or sets optional line-specific explanatory note.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the navigation reference to the parent entry.
    /// </summary>
    public GeneralLedgerEntry? GeneralLedgerEntry { get; set; }

    /// <summary>
    /// Gets or sets the navigation reference to the chart account.
    /// </summary>
    public ChartOfAccount? Account { get; set; }
}
