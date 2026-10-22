// <copyright file="InvoiceType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Defines the commercial and fiscal classification of an invoice or adjustment document.
/// </summary>
public enum InvoiceType
{

    /// <summary>
    /// Outgoing customer commercial invoice issued for goods sold or services rendered (corresponding to Italian SDI TD01 / Swiss customer billing).
    /// </summary>
    Sales = 1,

    /// <summary>
    /// Incoming supplier or vendor invoice received for acquired materials, merchandise, or third-party services.
    /// </summary>
    Purchase = 2,

    /// <summary>
    /// Credit note issued to decrease previously billed amounts, reverse erroneous invoices, or record customer returns and rebates (corresponding to Italian SDI TD04).
    /// </summary>
    CreditNote = 3,

    /// <summary>
    /// Debit note issued to charge additional amounts or upward adjustments to previously billed transactions (corresponding to Italian SDI TD05).
    /// </summary>
    DebitNote = 4,

    /// <summary>
    /// Fee note or pro-forma statement of fees (<c>Parcella</c> or <c>Avviso di parcella</c>) typically issued by independent professionals and consultants prior to definitive fiscal invoicing.
    /// </summary>
    FeeNote = 5,

}
