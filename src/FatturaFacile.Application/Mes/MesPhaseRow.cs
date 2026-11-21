// <copyright file="MesPhaseRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Mes;

/// <summary>
/// One row of the operator board: a phase, or an operation nobody took over yet (<see cref="PhaseId"/> null, state
/// <see cref="MesPhaseStatus.Available"/>). Times are as of the moment the board was received.
/// </summary>
/// <param name="PhaseId">The server phase identifier, null before the take-over reached the server.</param>
/// <param name="Version">The optimistic concurrency version of the phase.</param>
/// <param name="Status">The state.</param>
/// <param name="ResumeTo">The state a suspended phase resumes to.</param>
/// <param name="MachineMode">Attended or unattended running.</param>
/// <param name="OrderNumber">The production order number.</param>
/// <param name="ItemCode">The code of the item produced.</param>
/// <param name="ItemName">The name of the item produced.</param>
/// <param name="OperationId">The production order operation identifier.</param>
/// <param name="Sequence">The sequence of the operation in the routing.</param>
/// <param name="OperationName">The name of the operation.</param>
/// <param name="TargetQuantity">The quantity of the order.</param>
/// <param name="GoodQuantity">The good pieces declared.</param>
/// <param name="ScrapQuantity">The scrapped pieces declared.</param>
/// <param name="Times">The accumulated times.</param>
/// <param name="OpenInterval">The interval being measured, if any.</param>
/// <param name="Components">The components of the order.</param>
public sealed record MesPhaseRow(
    Guid? PhaseId,
    int Version,
    MesPhaseStatus Status,
    MesPhaseStatus? ResumeTo,
    MesMachineMode MachineMode,
    string OrderNumber,
    string ItemCode,
    string ItemName,
    Guid OperationId,
    int Sequence,
    string OperationName,
    decimal TargetQuantity,
    decimal GoodQuantity,
    decimal ScrapQuantity,
    MesTimeSummary Times,
    MesOpenInterval? OpenInterval,
    IReadOnlyList<MesComponent> Components)
{
    /// <summary>
    /// Gets a stable key of the row: the phase, or the operation of a row nobody took over.
    /// </summary>
    public string Key => PhaseId?.ToString() ?? ("op:" + OperationId);

    /// <summary>
    /// Gets the actions the board offers in the current state.
    /// </summary>
    public IReadOnlyList<MesAction> AllowedActions => MesStateMachine.AllowedActions(Status);
}
