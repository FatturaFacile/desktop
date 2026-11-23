// <copyright file="InputTaxAdjustmentType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Input tax adjustment types under Swiss VAT Act (LIVA Art. 30-33).
/// </summary>
public enum InputTaxAdjustmentType
{
    /// <summary>
    /// Self-consumption / private use (Art. 31 LIVA, Box 415).
    /// </summary>
    Autoconsumo = 0,

    /// <summary>
    /// Subsequent input tax deduction (Degrassazione / Einlageentsteuerung, Art. 32 LIVA, Box 410).
    /// </summary>
    Degrassazione = 1,

    /// <summary>
    /// Mixed-use proportional reduction (Art. 30 LIVA, Box 415).
    /// </summary>
    MixedUseReduction = 2,

    /// <summary>
    /// Subsidy-related input tax reduction (Art. 33(2) LIVA, Box 420).
    /// </summary>
    SubsidyReduction = 3,
}
