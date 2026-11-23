// <copyright file="ReceivableStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// The state of a receivable item, as the web-api reports it (overdue is derived from the due date).
/// </summary>
public enum ReceivableStatus
{
    /// <summary>
    /// Open and not yet due, nothing settled.
    /// </summary>
    Open = 0,

    /// <summary>
    /// Partly settled.
    /// </summary>
    PartiallyPaid = 1,

    /// <summary>
    /// Fully settled.
    /// </summary>
    Paid = 2,

    /// <summary>
    /// Open or partly paid and past its due date.
    /// </summary>
    Overdue = 3,

    /// <summary>
    /// Under dispute with the customer.
    /// </summary>
    Disputed = 4,

    /// <summary>
    /// Written off as a loss.
    /// </summary>
    WrittenOff = 5,
}
