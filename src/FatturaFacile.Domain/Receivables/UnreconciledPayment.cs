// <copyright file="UnreconciledPayment.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// A payment received on account that is not yet fully allocated to open items.
/// </summary>
/// <param name="LedgerEntryId">The ledger entry of the payment.</param>
/// <param name="Reference">The reference of the payment.</param>
/// <param name="Date">The date of the payment.</param>
/// <param name="Currency">The ISO currency.</param>
/// <param name="Amount">The amount received.</param>
/// <param name="Unallocated">The part not yet allocated.</param>
public sealed record UnreconciledPayment(
    Guid LedgerEntryId,
    string Reference,
    DateOnly Date,
    string Currency,
    decimal Amount,
    decimal Unallocated);
