// <copyright file="NormalBalance.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Normal accounting balance direction (Debit or Credit).
/// </summary>
public enum NormalBalance
{
    /// <summary>
    /// Debit balance (Dare / Soll).
    /// </summary>
    Debit = 0,

    /// <summary>
    /// Credit balance (Avere / Haben).
    /// </summary>
    Credit = 1,
}
