// <copyright file="AccountType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Financial classification of an account in the General Ledger chart of accounts.
/// </summary>
public enum AccountType
{
    /// <summary>
    /// Asset account (Attivo / Aktiven).
    /// </summary>
    Asset = 0,

    /// <summary>
    /// Liability account (Passivo / Passiven).
    /// </summary>
    Liability = 1,

    /// <summary>
    /// Equity account (Patrimonio Netto / Eigenkapital).
    /// </summary>
    Equity = 2,

    /// <summary>
    /// Revenue account (Ricavi / Ertrag).
    /// </summary>
    Revenue = 3,

    /// <summary>
    /// Expense account (Costi / Aufwand).
    /// </summary>
    Expense = 4,
}
