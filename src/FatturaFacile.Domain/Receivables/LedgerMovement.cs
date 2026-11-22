// <copyright file="LedgerMovement.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// One movement of the customer ledger.
/// </summary>
/// <param name="Date">The date.</param>
/// <param name="ValueDate">The value date.</param>
/// <param name="Kind">The kind of movement (invoice, allocation, ...).</param>
/// <param name="DocumentType">The document type.</param>
/// <param name="Reference">The reference.</param>
/// <param name="ReceivableItemId">The item, when any.</param>
/// <param name="AllocationId">The allocation, when any.</param>
/// <param name="Currency">The ISO currency.</param>
/// <param name="Debit">The debit.</param>
/// <param name="Credit">The credit.</param>
/// <param name="DueDate">The due date, when any.</param>
/// <param name="Status">The status of the item, when any.</param>
/// <param name="Balance">The running balance after the movement.</param>
public sealed record LedgerMovement(
    DateOnly Date,
    DateOnly? ValueDate,
    string Kind,
    string DocumentType,
    string Reference,
    Guid? ReceivableItemId,
    Guid? AllocationId,
    string Currency,
    decimal Debit,
    decimal Credit,
    DateOnly? DueDate,
    string? Status,
    decimal Balance);
