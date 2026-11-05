// <copyright file="GeneralLedgerLineDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer object for a General Ledger entry debit or credit line.
/// </summary>
public sealed class GeneralLedgerLineDto
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the parent entry identifier.
    /// </summary>
    public Guid GeneralLedgerEntryId { get; set; }

    /// <summary>
    /// Gets or sets the account identifier.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Gets or sets the account display label (code and name).
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sequential line number.
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Gets or sets the debit amount.
    /// </summary>
    public decimal Debit { get; set; }

    /// <summary>
    /// Gets or sets the credit amount.
    /// </summary>
    public decimal Credit { get; set; }

    /// <summary>
    /// Gets or sets the line description.
    /// </summary>
    public string? Description { get; set; }
}
