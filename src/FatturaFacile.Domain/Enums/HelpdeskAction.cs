// <copyright file="HelpdeskAction.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// A helpdesk action on a support ticket that is queued locally until the server accepts it (Mod 083).
/// </summary>
public enum HelpdeskAction
{
    /// <summary>
    /// Logs worked time on the ticket (contract deduction or billable time).
    /// </summary>
    LogTime = 0,

    /// <summary>
    /// Escalates the ticket to a Field Service work order.
    /// </summary>
    EscalateFieldService = 1,

    /// <summary>
    /// Escalates the ticket to an RMA (return merchandise authorization).
    /// </summary>
    EscalateRma = 2,
}
