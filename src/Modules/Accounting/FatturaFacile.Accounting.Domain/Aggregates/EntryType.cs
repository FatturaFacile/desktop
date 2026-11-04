// <copyright file="EntryType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Accounting.Domain.Aggregates;

/// <summary>
/// Specifies whether a general ledger accounting line represents a debit or credit entry.
/// </summary>
public enum EntryType
{
    /// <summary>
    /// Debit entry, recording an increase in assets/expenses or a decrease in liabilities/equity/revenue.
    /// </summary>
    Debit = 1,

    /// <summary>
    /// Credit entry, recording an increase in liabilities/equity/revenue or a decrease in assets/expenses.
    /// </summary>
    Credit = 2,
}
