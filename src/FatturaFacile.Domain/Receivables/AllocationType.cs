// <copyright file="AllocationType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// How an amount is applied to a receivable item.
/// </summary>
public enum AllocationType
{
    /// <summary>
    /// A payment, possibly taken from the unreconciled payments of the customer.
    /// </summary>
    Payment = 0,

    /// <summary>
    /// A credit note issued to the customer.
    /// </summary>
    CreditNote = 1,

    /// <summary>
    /// A debit adjustment that raises the open amount.
    /// </summary>
    DebitAdjustment = 2,

    /// <summary>
    /// A write-off of the remaining amount.
    /// </summary>
    WriteOff = 3,

    /// <summary>
    /// A discount granted after the invoice.
    /// </summary>
    Discount = 4,
}
