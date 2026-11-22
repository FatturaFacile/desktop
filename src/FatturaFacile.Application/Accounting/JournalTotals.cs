// <copyright file="JournalTotals.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// The debit and credit totals of an entry.
/// </summary>
/// <param name="Debit">The total debit (Dare).</param>
/// <param name="Credit">The total credit (Avere).</param>
public sealed record JournalTotals(decimal Debit, decimal Credit)
{
    /// <summary>
    /// Gets the difference debit minus credit (Differenza).
    /// </summary>
    public decimal Difference => Debit - Credit;

    /// <summary>
    /// Gets a value indicating whether the totals are equal and positive.
    /// </summary>
    public bool IsBalanced => Debit == Credit && Debit > 0m;
}
