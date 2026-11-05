// <copyright file="InvoiceStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Lifecycle status of an invoice.
/// </summary>
public enum InvoiceStatus
{
    /// <summary>
    /// Draft status, editable.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Issued status, final and non-editable.
    /// </summary>
    Issued = 1,

    /// <summary>
    /// Paid status.
    /// </summary>
    Paid = 2,

    /// <summary>
    /// Overdue status.
    /// </summary>
    Overdue = 3,

    /// <summary>
    /// Cancelled or credited status.
    /// </summary>
    Cancelled = 4,
}
