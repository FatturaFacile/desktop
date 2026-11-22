// <copyright file="LedgerBook.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Accounting;

/// <summary>
/// The parallel ledger books an entry can be posted to.
/// </summary>
public enum LedgerBook
{
    /// <summary>
    /// Book 01: statutory (civil code) accounting.
    /// </summary>
    Statutory = 1,

    /// <summary>
    /// Book 02: tax accounting.
    /// </summary>
    Tax = 2,

    /// <summary>
    /// Book 03: international (IFRS) accounting.
    /// </summary>
    Ifrs = 3,
}
