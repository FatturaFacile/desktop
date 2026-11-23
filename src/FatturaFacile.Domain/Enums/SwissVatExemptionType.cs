// <copyright file="SwissVatExemptionType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Distinguishes zero-rated/exempt supplies with input tax deduction (Art. 23 LIVA)
/// from exempt supplies without deduction (Art. 21 LIVA) and out-of-scope flows (Art. 18(2) LIVA).
/// </summary>
public enum SwissVatExemptionType
{
    /// <summary>
    /// Standard taxable supply.
    /// </summary>
    None = 0,

    /// <summary>
    /// Exempt with input tax deduction (exports, Art. 23 LIVA, Box 220/221/225).
    /// </summary>
    ExemptWithDeduction = 1,

    /// <summary>
    /// Exempt without input tax deduction (Art. 21 LIVA, Box 230).
    /// </summary>
    ExemptWithoutDeduction = 2,

    /// <summary>
    /// Out-of-scope / non-remuneration flows (Art. 18(2) LIVA, Box 900/910).
    /// </summary>
    OutOfScope = 3,
}
