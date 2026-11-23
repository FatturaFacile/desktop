// <copyright file="SettlementStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// The status of a settlement proposal of a supplier rebate.
/// </summary>
public enum SettlementStatus
{
    /// <summary>
    /// The proposal was created and can still be submitted or cancelled.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// The proposal waits for the approval.
    /// </summary>
    PendingApproval = 1,

    /// <summary>
    /// The proposal was approved and can be executed.
    /// </summary>
    Approved = 2,

    /// <summary>
    /// The proposal was executed.
    /// </summary>
    Settled = 3,

    /// <summary>
    /// The proposal was cancelled; its accruals are open again.
    /// </summary>
    Cancelled = 4,
}
