// <copyright file="InvoiceType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Classifies the commercial and fiscal document category of a billing instrument.
/// </summary>
public enum InvoiceType
{
    /// <summary>
    /// Standard commercial tax invoice representing goods or services rendered.
    /// </summary>
    Invoice = 0,

    /// <summary>
    /// Fiscal credit memo or credit note offsetting, refunding, or adjusting an existing invoice.
    /// </summary>
    CreditNote = 1,

    /// <summary>
    /// Preliminary or pro-forma invoice quote issued for review prior to fiscal invoicing.
    /// </summary>
    Proforma = 2,

    /// <summary>
    /// Advance payment request or deposit invoice against an order commitment.
    /// </summary>
    Deposit = 3,
}
