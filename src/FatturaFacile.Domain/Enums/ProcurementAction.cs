// <copyright file="ProcurementAction.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// The kind of a queued procurement fulfilment write (Mod 046).
/// </summary>
public enum ProcurementAction
{
    /// <summary>
    /// Transmit a purchase order to its supplier.
    /// </summary>
    DispatchOrder = 0,

    /// <summary>
    /// Record a supplier order confirmation.
    /// </summary>
    RecordConfirmation = 1,

    /// <summary>
    /// Remind a supplier of an overdue order.
    /// </summary>
    SendReminder = 2,

    /// <summary>
    /// Create the supplier order of a dropship sales order.
    /// </summary>
    CreateDropshipment = 3,

    /// <summary>
    /// Issue the custom transport document of a dropshipment.
    /// </summary>
    IssueDropshipDdt = 4,

    /// <summary>
    /// Record the delivery notice of a dropshipment.
    /// </summary>
    RecordDropshipDelivery = 5,
}
