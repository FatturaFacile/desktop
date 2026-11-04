// <copyright file="PaymentStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Invoicing.Domain.Enums;

/// <summary>
/// Indicates the settlement and lifecycle status of an invoice.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Unpaid invoice awaiting settlement.
    /// </summary>
    Unpaid = 1,

    /// <summary>
    /// Partially paid invoice with an outstanding balance.
    /// </summary>
    PartiallyPaid = 2,

    /// <summary>
    /// Fully paid and settled invoice.
    /// </summary>
    Paid = 3,

    /// <summary>
    /// Invoice past its due date without full settlement.
    /// </summary>
    Overdue = 4,

    /// <summary>
    /// Cancelled or voided invoice.
    /// </summary>
    Cancelled = 5,
}
