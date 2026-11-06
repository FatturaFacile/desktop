// <copyright file="InvoiceTotalRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Features.Reports.Builders;

/// <summary>
/// The rows of the "Totals" group.
/// </summary>
public enum InvoiceTotalRow
{
    /// <summary>
    /// Net total.
    /// </summary>
    Net = 0,

    /// <summary>
    /// Document discount.
    /// </summary>
    Discount = 1,

    /// <summary>
    /// VAT total.
    /// </summary>
    Tax = 2,

    /// <summary>
    /// Cash rounding (CHF 0.05).
    /// </summary>
    Rounding = 3,

    /// <summary>
    /// Gross total.
    /// </summary>
    Gross = 4,

    /// <summary>
    /// Italian withholding tax.
    /// </summary>
    Withholding = 5,

    /// <summary>
    /// VAT retained by the public administration (split payment).
    /// </summary>
    SplitPayment = 6,

    /// <summary>
    /// Italian stamp duty.
    /// </summary>
    StampDuty = 7,

    /// <summary>
    /// Amount payable.
    /// </summary>
    Payable = 8,
}
