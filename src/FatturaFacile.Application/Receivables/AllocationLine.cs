// <copyright file="AllocationLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Receivables;

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// One allocation of an amount to an item.
/// </summary>
/// <param name="ReceivableItemId">The item.</param>
/// <param name="Type">The kind of allocation.</param>
/// <param name="Amount">The amount, positive.</param>
/// <param name="ValueDate">The value date.</param>
/// <param name="SourceId">The ledger entry of the unreconciled payment or the credit note invoice.</param>
/// <param name="Reference">A reference.</param>
/// <param name="Notes">Free notes.</param>
public sealed record AllocationLine(
    Guid ReceivableItemId,
    AllocationType Type,
    decimal Amount,
    DateOnly ValueDate,
    string? SourceId,
    string? Reference,
    string? Notes);
