// <copyright file="PaymentStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Settlement status of a payable or receivable document.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Document is completely unpaid.
    /// </summary>
    Unpaid = 0,

    /// <summary>
    /// Document has been partially settled.
    /// </summary>
    PartiallyPaid = 1,

    /// <summary>
    /// Document has been completely settled.
    /// </summary>
    Paid = 2,
}
