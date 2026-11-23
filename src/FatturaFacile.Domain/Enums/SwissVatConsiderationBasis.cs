// <copyright file="SwissVatConsiderationBasis.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Swiss VAT consideration basis (Art. 39 LIVA).
/// </summary>
public enum SwissVatConsiderationBasis
{
    /// <summary>
    /// Agreed consideration (vereinbart / fatturato).
    /// </summary>
    Agreed = 0,

    /// <summary>
    /// Received consideration (vereinnahmt / incassato).
    /// </summary>
    Received = 1,
}
