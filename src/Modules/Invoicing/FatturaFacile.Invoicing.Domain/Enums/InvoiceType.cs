// <copyright file="InvoiceType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Invoicing.Domain.Enums;

/// <summary>
/// Specifies the business format and legal classification of an invoice.
/// </summary>
public enum InvoiceType
{
    /// <summary>
    /// Standard commercial invoice.
    /// </summary>
    Standard = 1,

    /// <summary>
    /// Italian Sistema di Interscambio (SDI) XML FatturaPA electronic invoice.
    /// </summary>
    SdiElectronic = 2,

    /// <summary>
    /// Swiss QR-bill compliant invoice with embedded payment slip.
    /// </summary>
    ChQrBill = 3,

    /// <summary>
    /// Credit note issuing a refund or reduction on a prior invoice.
    /// </summary>
    CreditNote = 4,

    /// <summary>
    /// Debit note requesting additional payment on an existing invoice.
    /// </summary>
    DebitNote = 5,

    /// <summary>
    /// Proforma estimated invoice before delivery of goods or services.
    /// </summary>
    Proforma = 6,
}
