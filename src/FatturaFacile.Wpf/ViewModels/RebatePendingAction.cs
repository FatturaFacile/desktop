// <copyright file="RebatePendingAction.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A destructive or irreversible action of the rebate cockpit that waits for the confirmation of the user.
/// </summary>
internal enum RebatePendingAction
{
    /// <summary>
    /// Nothing waits for a confirmation.
    /// </summary>
    None = 0,

    /// <summary>
    /// The execution of an approved settlement proposal.
    /// </summary>
    ExecuteProposal = 1,

    /// <summary>
    /// The cancellation of a settlement proposal (its accruals become open again).
    /// </summary>
    CancelProposal = 2,

    /// <summary>
    /// The deletion of a draft agreement.
    /// </summary>
    DeleteAgreement = 3,
}
