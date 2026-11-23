// <copyright file="ReceivableAllocation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// An amount applied to a receivable item (payment, credit note, adjustment, write-off or discount).
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="ReceivableItemId">The item it settles.</param>
/// <param name="Type">The kind of allocation.</param>
/// <param name="Origin">Where it came from (manual, bank, ...).</param>
/// <param name="SourceId">The identifier of the source document, when any.</param>
/// <param name="SourceReference">The reference of the source document, when any.</param>
/// <param name="Amount">The amount (negative for a debit adjustment).</param>
/// <param name="ValueDate">The value date.</param>
/// <param name="Notes">Free notes.</param>
/// <param name="ReversedAt">The instant it was reversed, when it was.</param>
public sealed record ReceivableAllocation(
    Guid Id,
    Guid ReceivableItemId,
    AllocationType Type,
    string Origin,
    string? SourceId,
    string? SourceReference,
    decimal Amount,
    DateOnly ValueDate,
    string? Notes,
    DateTime? ReversedAt)
{
    /// <summary>
    /// Gets a value indicating whether the allocation was reversed.
    /// </summary>
    public bool IsReversed => ReversedAt is not null;
}
