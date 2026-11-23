// <copyright file="SwissVatFrequency.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Swiss VAT reporting interval.
/// </summary>
public enum SwissVatFrequency
{
    /// <summary>
    /// Quarterly reporting (default for Effective method).
    /// </summary>
    Quarterly = 0,

    /// <summary>
    /// Semi-annual reporting (default for Saldosteuersatz).
    /// </summary>
    Semiannual = 1,

    /// <summary>
    /// Monthly reporting.
    /// </summary>
    Monthly = 2,
}
