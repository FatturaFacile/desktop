// <copyright file="BezugsteuerStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Review lifecycle status of a Bezugsteuer entry.
/// </summary>
public enum BezugsteuerStatus
{
    /// <summary>
    /// Automatically detected from a cross-border supplier invoice.
    /// </summary>
    Detected = 0,

    /// <summary>
    /// Confirmed for declaration in Box 382/383.
    /// </summary>
    Confirmed = 1,

    /// <summary>
    /// Excluded from declaration.
    /// </summary>
    Excluded = 2,

    /// <summary>
    /// Declared on a finalized Swiss VAT return.
    /// </summary>
    Declared = 3,
}
