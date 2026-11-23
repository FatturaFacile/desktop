// <copyright file="SettlementAction.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// A step of the settlement workflow.
/// </summary>
public enum SettlementAction
{
    /// <summary>
    /// Sends a draft proposal in approval.
    /// </summary>
    Submit = 0,

    /// <summary>
    /// Approves a proposal that waits for the approval.
    /// </summary>
    Approve = 1,

    /// <summary>
    /// Executes an approved proposal (credit note request, charge invoice or set-off).
    /// </summary>
    Execute = 2,

    /// <summary>
    /// Cancels a proposal that was not executed.
    /// </summary>
    Cancel = 3,
}
