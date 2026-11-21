// <copyright file="MesStateMachine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.Frozen;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Mes;

/// <summary>
/// The phase state machine of the MES operator board, the same table the web-api enforces: which actions start from which
/// state and where they lead. The client uses it to enable only valid actions and to project queued commands offline; the
/// server remains the authority.
/// </summary>
public static class MesStateMachine
{
    private static readonly FrozenDictionary<MesAction, MesPhaseStatus[]> AllowedFrom = new Dictionary<MesAction, MesPhaseStatus[]>
    {
        [MesAction.TakeOver] = [MesPhaseStatus.Available, MesPhaseStatus.Assigned, MesPhaseStatus.Suspended],
        [MesAction.StartSetup] = [MesPhaseStatus.Assigned],
        [MesAction.StartProduction] = [MesPhaseStatus.Assigned, MesPhaseStatus.Setup, MesPhaseStatus.Running],
        [MesAction.Suspend] = [MesPhaseStatus.Setup, MesPhaseStatus.Running],
        [MesAction.Resume] = [MesPhaseStatus.Suspended],
        [MesAction.DeclareProduction] = [MesPhaseStatus.Running, MesPhaseStatus.Suspended],
        [MesAction.ConsumeMaterial] = [MesPhaseStatus.Setup, MesPhaseStatus.Running, MesPhaseStatus.Suspended],
        [MesAction.Close] = [MesPhaseStatus.Running, MesPhaseStatus.Suspended],
    }.ToFrozenDictionary();

    /// <summary>
    /// Determines whether the action may start from the state.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="status">The current state.</param>
    /// <returns><see langword="true"/> when the action is allowed.</returns>
    public static bool IsAllowed(MesAction action, MesPhaseStatus status) => AllowedFrom[action].Contains(status);

    /// <summary>
    /// Lists the actions the board offers in a state, in display order.
    /// </summary>
    /// <param name="status">The state.</param>
    /// <returns>The allowed actions.</returns>
    public static IReadOnlyList<MesAction> AllowedActions(MesPhaseStatus status) =>
        [.. Enum.GetValues<MesAction>().Where(action => IsAllowed(action, status))];

    /// <summary>
    /// Gives the state an action leads to.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="status">The current state.</param>
    /// <param name="resumeTo">The state a suspended phase resumes to.</param>
    /// <returns>The next state.</returns>
    public static MesPhaseStatus Next(MesAction action, MesPhaseStatus status, MesPhaseStatus? resumeTo) => action switch
    {
        MesAction.TakeOver => status == MesPhaseStatus.Available ? MesPhaseStatus.Assigned : status,
        MesAction.StartSetup => MesPhaseStatus.Setup,
        MesAction.StartProduction => MesPhaseStatus.Running,
        MesAction.Suspend => MesPhaseStatus.Suspended,
        MesAction.Resume => resumeTo ?? MesPhaseStatus.Running,
        MesAction.DeclareProduction => status,
        MesAction.ConsumeMaterial => status,
        MesAction.Close => MesPhaseStatus.Closed,
        _ => status,
    };
}
