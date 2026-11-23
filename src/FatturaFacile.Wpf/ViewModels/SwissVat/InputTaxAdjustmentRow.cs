// <copyright file="InputTaxAdjustmentRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels.SwissVat;

/// <summary>
/// Display row for a LIVA Art. 30-33 input tax adjustment.
/// </summary>
/// <param name="Id">Adjustment identifier.</param>
/// <param name="EffectiveDate">Effective date.</param>
/// <param name="Type">Adjustment type.</param>
/// <param name="AfcBox">Target AFC box (410, 415, 420).</param>
/// <param name="LegalBasis">Legal basis citation.</param>
/// <param name="Description">Description.</param>
/// <param name="AmountChf">Adjustment amount in CHF.</param>
/// <param name="Status">Status.</param>
internal sealed record InputTaxAdjustmentRow(
    Guid Id,
    string EffectiveDate,
    string Type,
    string AfcBox,
    string LegalBasis,
    string Description,
    string AmountChf,
    string Status);
