// <copyright file="PaymentStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies the monetary settlement and collection status of an invoice or billing document.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// The invoice has been issued but no payment has been received yet.
    /// </summary>
    Unpaid = 0,

    /// <summary>
    /// The invoice balance has been settled in full.
    /// </summary>
    Paid = 1,

    /// <summary>
    /// Partial installments or downpayments have been recorded, but an unpaid balance remains.
    /// </summary>
    PartiallyPaid = 2,

    /// <summary>
    /// The document due date has lapsed without receiving the full payable amount.
    /// </summary>
    Overdue = 3,

    /// <summary>
    /// The invoice or receivable claim has been voided, cancelled, or written off.
    /// </summary>
    Cancelled = 4,
}
