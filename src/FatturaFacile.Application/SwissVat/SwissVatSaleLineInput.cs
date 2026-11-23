// <copyright file="SwissVatSaleLineInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Represents a sales line item input for Swiss VAT period calculation.
/// </summary>
/// <param name="NetAmount">Net line amount in CHF.</param>
/// <param name="TaxRate">Applied VAT rate percentage.</param>
/// <param name="TaxAmount">Optional explicit VAT amount in CHF.</param>
/// <param name="ExemptionType">Zero/exempt classification.</param>
/// <param name="AfcBox">Optional target AFC box (e.g., 220, 221, 230, 303, 900).</param>
/// <param name="SssRate">Optional Saldosteuersatz rate override for multi-activity companies.</param>
/// <param name="IsCreditNote">True when this line belongs to a credit note.</param>
public sealed record SwissVatSaleLineInput(
    decimal NetAmount,
    decimal TaxRate,
    decimal? TaxAmount = null,
    SwissVatExemptionType ExemptionType = SwissVatExemptionType.None,
    string? AfcBox = null,
    decimal? SssRate = null,
    bool IsCreditNote = false);
