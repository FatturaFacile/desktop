// <copyright file="MesLocalProjection.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Mes;

/// <summary>
/// Projects the commands still waiting in the outbox onto the board the server last sent, so the operator sees the effect of
/// what was done offline (state, open interval, counters, declared pieces) before the server has accepted it. The projection
/// follows the same state machine and time rules as the server, from the instants the operator really acted.
/// </summary>
public static class MesLocalProjection
{
    /// <summary>
    /// Applies the pending commands to the rows of a board and advances the times of the touched rows to now.
    /// </summary>
    /// <param name="rows">The rows as the server (or the cache) gave them.</param>
    /// <param name="serverTimeUtc">The instant the rows' times are computed for.</param>
    /// <param name="pending">The commands not yet accepted.</param>
    /// <param name="nowUtc">The current instant.</param>
    /// <param name="advanceUntouched">Whether rows without pending commands are advanced to now too, which a board read from the
    /// cache needs because its times are as old as the cache.</param>
    /// <returns>The projected rows, in the same order.</returns>
    public static IReadOnlyList<MesPhaseRow> Project(IReadOnlyList<MesPhaseRow> rows, DateTime serverTimeUtc, IEnumerable<MesCommand> pending, DateTime nowUtc, bool advanceUntouched = false)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(pending);

        var byOperation = pending.GroupBy(command => command.OperationId).ToDictionary(group => group.Key, group => group.OrderBy(command => command.OccurredAtUtc).ToList());
        List<MesPhaseRow> projected = [];

        foreach (var row in rows)
        {
            if (byOperation.TryGetValue(row.OperationId, out var commands))
            {
                projected.Add(ProjectRow(row, serverTimeUtc, commands, nowUtc));
            }
            else
            {
                projected.Add(advanceUntouched ? row with { Times = row.Times.Advance(row.OpenInterval, (nowUtc - serverTimeUtc).TotalSeconds) } : row);
            }
        }

        return projected;
    }

    private static MesPhaseRow ProjectRow(MesPhaseRow row, DateTime serverTimeUtc, List<MesCommand> commands, DateTime nowUtc)
    {
        var cursor = serverTimeUtc;
        var current = row;

        foreach (var command in commands)
        {
            if (!MesStateMachine.IsAllowed(command.Action, current.Status))
            {
                // The server will refuse it too: it is a conflict to show, not a state to show.
                continue;
            }

            var at = command.OccurredAtUtc > cursor ? command.OccurredAtUtc : cursor;
            current = Apply(current with { Times = current.Times.Advance(current.OpenInterval, (at - cursor).TotalSeconds) }, command, at);
            cursor = at;
        }

        var end = nowUtc > cursor ? nowUtc : cursor;

        return current with { Times = current.Times.Advance(current.OpenInterval, (end - cursor).TotalSeconds) };
    }

    private static MesPhaseRow Apply(MesPhaseRow row, MesCommand command, DateTime at)
    {
        var next = MesStateMachine.Next(command.Action, row.Status, row.ResumeTo);

        return command.Action switch
        {
            MesAction.TakeOver => row with { Status = next },
            MesAction.StartSetup => row with { Status = next, OpenInterval = new MesOpenInterval(MesTimeCategory.Setup, at, MachineAttended: true, Reason: null) },
            MesAction.StartProduction => StartRun(row, command, next, at),
            MesAction.Suspend => row with
            {
                Status = next,
                ResumeTo = row.Status,
                OpenInterval = new MesOpenInterval(MesTimeCategory.Suspension, at, MachineAttended: false, Reason(command)),
            },
            MesAction.Resume => Resume(row, next, at),
            MesAction.DeclareProduction => row with
            {
                GoodQuantity = row.GoodQuantity + command.Amount(MesCommand.GoodField),
                ScrapQuantity = row.ScrapQuantity + command.Amount(MesCommand.ScrapField),
            },
            MesAction.ConsumeMaterial => Consume(row, command),
            MesAction.Close => row with { Status = next, ResumeTo = null, OpenInterval = null },
            _ => row,
        };
    }

    private static MesPhaseRow StartRun(MesPhaseRow row, MesCommand command, MesPhaseStatus next, DateTime at)
    {
        var mode = MesWire.TryParse(command.Text(MesCommand.MachineModeField), out MesMachineMode parsed) ? parsed : MesMachineMode.Attended;

        return row with
        {
            Status = next,
            MachineMode = mode,
            OpenInterval = new MesOpenInterval(MesTimeCategory.Run, at, mode == MesMachineMode.Attended, Reason: null),
        };
    }

    private static MesPhaseRow Resume(MesPhaseRow row, MesPhaseStatus next, DateTime at)
    {
        var open = next == MesPhaseStatus.Setup
            ? new MesOpenInterval(MesTimeCategory.Setup, at, MachineAttended: true, Reason: null)
            : new MesOpenInterval(MesTimeCategory.Run, at, row.MachineMode == MesMachineMode.Attended, Reason: null);

        return row with { Status = next, ResumeTo = null, OpenInterval = open };
    }

    private static MesPhaseRow Consume(MesPhaseRow row, MesCommand command)
    {
        var consumed = command.Amount(MesCommand.QuantityField);
        var itemId = command.Text(MesCommand.ItemField);
        var components = row.Components
            .Select(component => string.Equals(component.ItemId.ToString("D", CultureInfo.InvariantCulture), itemId, StringComparison.Ordinal)
                ? component with { ConsumedQuantity = component.ConsumedQuantity + consumed }
                : component)
            .ToList();

        return row with { Components = components };
    }

    private static MesSuspensionReason? Reason(MesCommand command) =>
        MesWire.TryParse(command.Text(MesCommand.ReasonField), out MesSuspensionReason reason) ? reason : null;
}
