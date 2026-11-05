// <copyright file="InvoiceType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Type classification of an invoice or credit document.
/// </summary>
public enum InvoiceType
{
    /// <summary>
    /// Standard customer sales invoice.
    /// </summary>
    Standard = 0,

    /// <summary>
    /// Proforma quotation/invoice.
    /// </summary>
    Proforma = 1,

    /// <summary>
    /// Credit note (nota di credito / nota di accredito).
    /// </summary>
    CreditNote = 2,

    /// <summary>
    /// Debit note (nota di debito).
    /// </summary>
    DebitNote = 3,
}
