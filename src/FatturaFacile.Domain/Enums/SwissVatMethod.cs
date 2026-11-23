// <copyright file="SwissVatMethod.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Swiss VAT reporting method under LIVA / MWSTG / LTVA.
/// </summary>
public enum SwissVatMethod
{
    /// <summary>
    /// Effective method (Form 050) with full input tax deduction.
    /// </summary>
    Effective = 0,

    /// <summary>
    /// Flat-rate / net tax rate method (Saldosteuersatz, Form 051).
    /// </summary>
    Saldosteuersatz = 1,
}
