// <copyright file="RoutingLeadTimeCostCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Domain.Manufacturing;

/// <summary>
/// Pure port of the web-api RoutingLeadTimeCostCalculator: the lead time of a routing (queue, setup, run, wait and move time with
/// optional operation overlap) and its cost (setup, machine run, direct labour, overhead, subcontracting). Material cost needs the
/// standard cost of the components, which is not mirrored offline, so it stays zero. The calendar projection of the server is not ported.
/// </summary>
public static class RoutingLeadTimeCostCalculator
{
    private const string Subcontracting = "external_subcontracting";
    private const string InternalMachining = "internal_machining";
    private const int CostDecimals = 4;
    private const decimal MinutesPerHour = 60m;

    /// <summary>
    /// Calculates the lead time and the cost of a routing.
    /// </summary>
    /// <param name="routing">The routing with its operations.</param>
    /// <param name="workCenters">The work centres of the tenant by identifier.</param>
    /// <param name="machines">The machines of the tenant by identifier.</param>
    /// <param name="quantity">The produced quantity (positive).</param>
    /// <param name="includeOverlap">Whether the transfer-batch overlap of the operations is applied.</param>
    /// <returns>The calculation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The quantity is not positive.</exception>
    /// <exception cref="InvalidOperationException">The routing has no operations or one of them has an unknown work centre.</exception>
    public static RoutingCalculation Calculate(
        MfgRouting routing,
        IReadOnlyDictionary<Guid, MfgWorkCenter> workCenters,
        IReadOnlyDictionary<Guid, MfgMachineCenter> machines,
        decimal quantity,
        bool includeOverlap = true)
    {
        ArgumentNullException.ThrowIfNull(routing);
        ArgumentNullException.ThrowIfNull(workCenters);
        ArgumentNullException.ThrowIfNull(machines);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        var operations = routing.Operations.OrderBy(o => o.Sequence).ToList();
        if (operations.Count == 0)
        {
            throw new InvalidOperationException("No operation steps are defined.");
        }

        var qty = (double)quantity;
        var steps = operations.ConvertAll(o => Step(o, workCenters, machines, quantity, qty));
        var timeline = Timeline(steps, qty, includeOverlap);
        var serial = steps.Sum(s => s.Duration);

        var rows = new List<RoutingStepResult>(steps.Count);
        for (var i = 0; i < steps.Count; i++)
        {
            var s = steps[i];
            rows.Add(new RoutingStepResult(
                s.OperationId,
                s.Sequence,
                s.OperationCode,
                s.OperationType,
                s.WorkCenterId,
                Round3(s.Queue),
                Round3(s.Setup),
                Round3(s.Run),
                Round3(s.Post),
                Round3(s.Move),
                Round3(s.Duration),
                Round3(timeline.Starts[i]),
                Round3(timeline.Ends[i]),
                timeline.Overlapped[i],
                Round(s.Cost)));
        }

        var cost = Round(new RoutingCostBreakdown(
            steps.Sum(s => s.Cost.Setup),
            steps.Sum(s => s.Cost.MachineRun),
            steps.Sum(s => s.Cost.DirectLabor),
            steps.Sum(s => s.Cost.Overhead),
            steps.Sum(s => s.Cost.Subcontracting),
            steps.Sum(s => s.Cost.Material)));

        return new RoutingCalculation(
            decimal.Round(quantity, CostDecimals, MidpointRounding.AwayFromZero),
            includeOverlap,
            Round3(timeline.Total),
            Round3(serial),
            Round3(serial - timeline.Total),
            cost,
            decimal.Round(steps.Sum(s => s.Cost.Total) / quantity, CostDecimals, MidpointRounding.AwayFromZero),
            rows);
    }

    private static StepFigures Step(
        MfgRoutingOperation op,
        IReadOnlyDictionary<Guid, MfgWorkCenter> workCenters,
        IReadOnlyDictionary<Guid, MfgMachineCenter> machines,
        decimal quantity,
        double qty)
    {
        if (!workCenters.TryGetValue(op.WorkCenterId, out var wc))
        {
            throw new InvalidOperationException("The work centre of operation " + op.OperationCode + " is unknown.");
        }

        MfgMachineCenter? machine = null;
        if (op.MachineCenterId is { } machineId)
        {
            _ = machines.TryGetValue(machineId, out machine);
        }

        var type = ResolveType(op.OperationType);
        var subcontracted = string.Equals(type, Subcontracting, StringComparison.Ordinal) || op.IsSubcontracted;
        var parallel = Math.Max(1, machine?.ParallelCapacityUnits ?? wc.ParallelCapacityUnits);
        var efficiency = machine?.EfficiencyFactorPercent ?? wc.EfficiencyRate;
        efficiency = efficiency > 0m ? efficiency : 100m;
        var divisor = parallel * (double)efficiency / 100d;

        var runPerUnit = (double)op.RunTimeMinutesPerUnit;
        var run = runPerUnit * qty / divisor;
        var laborPerUnit = op.LaborTimeMinutesPerUnit > 0m ? (double)op.LaborTimeMinutesPerUnit : runPerUnit;
        var labor = laborPerUnit * qty / divisor;
        var queue = op.QueueTimeMinutes > 0m ? (double)op.QueueTimeMinutes : (double)wc.DefaultQueueWaitMinutes;
        var move = op.MoveTimeMinutes > 0m ? (double)op.MoveTimeMinutes : (double)wc.DefaultMoveTimeMinutes;
        var setup = (double)op.SetupTimeMinutes;
        var post = (double)op.PostOperationWaitMinutes;
        var overlap = (double)(op.OverlapTransferBatchQty ?? 0m);

        var machineRate = machine?.HourlyMachineRunCostRateOverride ?? wc.HourlyMachineRunCostRate;
        var machineCosted = string.Equals(type, InternalMachining, StringComparison.Ordinal) && !subcontracted;
        var cost = subcontracted
            ? new RoutingCostBreakdown(0m, 0m, 0m, 0m, op.SubcontractUnitCost * quantity, 0m)
            : new RoutingCostBreakdown(
                Hours(setup) * wc.HourlySetupCostRate,
                MachineRunCost(machineCosted, run, machineRate),
                Hours(labor) * wc.HourlyCostRate * Math.Max(1, op.RequiredOperators),
                Hours(setup + run) * wc.HourlyOverheadRate,
                0m,
                0m);

        return new StepFigures(
            op.Id,
            op.Sequence,
            op.OperationCode,
            type,
            op.WorkCenterId,
            queue,
            setup,
            run,
            post,
            move,
            overlap > 0d && overlap < qty ? overlap : null,
            cost);
    }

    private static Timing Timeline(List<StepFigures> steps, double qty, bool includeOverlap)
    {
        var starts = new double[steps.Count];
        var ends = new double[steps.Count];
        var overlapped = new bool[steps.Count];
        var start = 0d;
        var floor = 0d;

        for (var i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            var end = Math.Max(start + step.Duration, floor);
            starts[i] = start;
            ends[i] = end;

            var batch = includeOverlap ? step.OverlapBatch : null;
            if (batch is { } size && i + 1 < steps.Count)
            {
                var next = steps[i + 1];
                overlapped[i] = true;
                start += step.Queue + step.Setup + (step.Run * size / qty) + step.Post + step.Move;
                floor = end + (next.Run * size / qty) + next.Post + next.Move;
            }
            else
            {
                start = end;
                floor = 0d;
            }
        }

        return new Timing(starts, ends, overlapped, ends.Max());
    }

    private static decimal MachineRunCost(bool machineCosted, double run, decimal rate) => machineCosted ? Hours(run) * rate : 0m;

    private static string ResolveType(string operationType)
    {
        var known = string.Equals(operationType, "manual_assembly", StringComparison.Ordinal) || string.Equals(operationType, Subcontracting, StringComparison.Ordinal);
        return known ? operationType : InternalMachining;
    }

    private static decimal Hours(double minutes) => (decimal)minutes / MinutesPerHour;

    private static double Round3(double value) => Math.Round(value, 3, MidpointRounding.AwayFromZero);

    private static decimal Round4(decimal value) => decimal.Round(value, CostDecimals, MidpointRounding.AwayFromZero);

    private static RoutingCostBreakdown Round(RoutingCostBreakdown c) =>
        new(Round4(c.Setup), Round4(c.MachineRun), Round4(c.DirectLabor), Round4(c.Overhead), Round4(c.Subcontracting), Round4(c.Material));

    private sealed record StepFigures(
        Guid OperationId,
        int Sequence,
        string OperationCode,
        string OperationType,
        Guid WorkCenterId,
        double Queue,
        double Setup,
        double Run,
        double Post,
        double Move,
        double? OverlapBatch,
        RoutingCostBreakdown Cost)
    {
        public double Duration => Queue + Setup + Run + Post + Move;
    }

    private sealed record Timing(double[] Starts, double[] Ends, bool[] Overlapped, double Total);
}
