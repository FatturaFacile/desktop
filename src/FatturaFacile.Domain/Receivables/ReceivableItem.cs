// <copyright file="ReceivableItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// An open item of the customer ledger: one instalment of an invoice with what is still to be collected.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="CustomerId">The customer.</param>
/// <param name="InvoiceId">The invoice, when any.</param>
/// <param name="Sequence">The instalment number.</param>
/// <param name="DocumentType">The document type (invoice, credit note, ...).</param>
/// <param name="DocumentNumber">The document number.</param>
/// <param name="Currency">The ISO currency.</param>
/// <param name="OriginalAmount">The amount at issue.</param>
/// <param name="OpenAmount">The amount still open.</param>
/// <param name="IssueDate">The issue date.</param>
/// <param name="DueDate">The due date.</param>
/// <param name="ValueDate">The date of the last settlement.</param>
/// <param name="Status">The status.</param>
/// <param name="DaysPastDue">The days past the due date at the time of the answer.</param>
/// <param name="DisputeReason">The reason of the dispute, when disputed.</param>
/// <param name="Allocations">The allocations, when loaded.</param>
public sealed record ReceivableItem(
    Guid Id,
    Guid CustomerId,
    Guid? InvoiceId,
    int Sequence,
    string DocumentType,
    string DocumentNumber,
    string Currency,
    decimal OriginalAmount,
    decimal OpenAmount,
    DateOnly IssueDate,
    DateOnly DueDate,
    DateOnly? ValueDate,
    ReceivableStatus Status,
    int DaysPastDue,
    string? DisputeReason,
    IReadOnlyList<ReceivableAllocation> Allocations)
{
    /// <summary>
    /// Gets the amount settled so far.
    /// </summary>
    public decimal SettledAmount => OriginalAmount - OpenAmount;

    /// <summary>
    /// Gets a value indicating whether something is still to be collected on the item.
    /// </summary>
    public bool IsOpen => OpenAmount > 0m && Status != ReceivableStatus.WrittenOff;

    /// <summary>
    /// Gets a value indicating whether the item is disputed.
    /// </summary>
    public bool IsDisputed => Status == ReceivableStatus.Disputed;

    /// <summary>
    /// Computes the days past the due date on a day; an item that is not yet due has none.
    /// </summary>
    /// <param name="today">The day.</param>
    /// <returns>The days past due, never negative.</returns>
    public int DaysPastDueOn(DateOnly today) => Math.Max(0, today.DayNumber - DueDate.DayNumber);
}
