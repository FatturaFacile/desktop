// <copyright file="AccountCategory.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Defines the fundamental accounting classification of a general ledger account according to standard double-entry bookkeeping frameworks.
/// </summary>
public enum AccountCategory
{

    /// <summary>
    /// Economic resources owned or controlled by the entity expected to produce future economic benefits (e.g., cash, receivables, inventory, property, plant, and equipment).
    /// </summary>
    Asset = 1,

    /// <summary>
    /// Present obligations of the entity arising from past events, settlement of which is expected to result in an outflow of resources (e.g., payables, debts, tax liabilities).
    /// </summary>
    Liability = 2,

    /// <summary>
    /// Residual interest in the assets of the entity after deducting all its liabilities (e.g., share capital, retained earnings, reserves).
    /// </summary>
    Equity = 3,

    /// <summary>
    /// Inflows or increases of assets or decreases of liabilities resulting in increases in equity, arising in the course of ordinary operating activities (e.g., sales, service fees).
    /// </summary>
    Revenue = 4,

    /// <summary>
    /// Outflows or depletion of assets or incurrence of liabilities resulting in decreases in equity, incurred in ordinary business operations (e.g., cost of goods sold, operating overheads, wages).
    /// </summary>
    Expense = 5,

}
