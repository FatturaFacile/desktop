// <copyright file="JournalViolation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// The ways a journal entry can break the double-entry rules.
/// </summary>
public enum JournalViolation
{
    /// <summary>
    /// Fewer than two lines carry an amount.
    /// </summary>
    TooFewLines = 0,

    /// <summary>
    /// A line has a negative debit or credit.
    /// </summary>
    NegativeAmount = 1,

    /// <summary>
    /// A line carries both a debit and a credit.
    /// </summary>
    BothSides = 2,

    /// <summary>
    /// A line carries neither a debit nor a credit.
    /// </summary>
    EmptyLine = 3,

    /// <summary>
    /// A line has no account.
    /// </summary>
    MissingAccount = 4,

    /// <summary>
    /// Total debit differs from total credit.
    /// </summary>
    Unbalanced = 5,

    /// <summary>
    /// The entry total is zero.
    /// </summary>
    NonPositiveTotal = 6,

    /// <summary>
    /// The entry is already posted and can no longer change.
    /// </summary>
    AlreadyPosted = 7,

    /// <summary>
    /// Only a posted entry can be reversed.
    /// </summary>
    NotPosted = 8,

    /// <summary>
    /// The entry is itself a reversal, or has already been reversed.
    /// </summary>
    CannotReverse = 9,
}
