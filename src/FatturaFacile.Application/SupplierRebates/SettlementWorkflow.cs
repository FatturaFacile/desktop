// <copyright file="SettlementWorkflow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// The state machine of a settlement proposal, the same as the web-api: draft, pending approval, approved, settled (or cancelled).
/// Submit moves a draft to approval, approve an approval to approved, execute an approved proposal to settled; cancel is possible
/// until the proposal is settled.
/// </summary>
public static class SettlementWorkflow
{
    private static readonly SettlementAction[] None = [];

    private static readonly Dictionary<SettlementStatus, SettlementAction[]> Allowed = new()
    {
        [SettlementStatus.Draft] = [SettlementAction.Submit, SettlementAction.Cancel],
        [SettlementStatus.PendingApproval] = [SettlementAction.Approve, SettlementAction.Cancel],
        [SettlementStatus.Approved] = [SettlementAction.Execute, SettlementAction.Cancel],
    };

    /// <summary>
    /// Gets the actions a proposal in a status accepts.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <returns>The allowed actions, none for a settled or cancelled proposal.</returns>
    public static IReadOnlyList<SettlementAction> AllowedActions(SettlementStatus status) => Allowed.GetValueOrDefault(status, None);

    /// <summary>
    /// Checks whether a proposal in a status accepts an action.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <param name="action">The action.</param>
    /// <returns>True when the action is allowed.</returns>
    public static bool CanApply(SettlementStatus status, SettlementAction action) => AllowedActions(status).Contains(action);

    /// <summary>
    /// Gets the status a proposal reaches after an action.
    /// </summary>
    /// <param name="status">The current status.</param>
    /// <param name="action">The action.</param>
    /// <returns>The new status, or null when the action is not allowed.</returns>
    public static SettlementStatus? Advance(SettlementStatus status, SettlementAction action) =>
        CanApply(status, action) ? Next(action) : null;

    private static SettlementStatus Next(SettlementAction action) => action switch
    {
        SettlementAction.Submit => SettlementStatus.PendingApproval,
        SettlementAction.Approve => SettlementStatus.Approved,
        SettlementAction.Execute => SettlementStatus.Settled,
        SettlementAction.Cancel => SettlementStatus.Cancelled,
        _ => SettlementStatus.Cancelled,
    };
}
