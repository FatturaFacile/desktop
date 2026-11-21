// <copyright file="RoutingLeadTimeCostCalculatorTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Manufacturing;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the offline routing lead time and cost calculator (a port of the web-api RoutingLeadTimeCostCalculator).
/// </summary>
public sealed class RoutingLeadTimeCostCalculatorTests
{
    private static readonly Guid CenterId = ManufacturingTestData.WorkCenterA;

    private readonly Dictionary<Guid, MfgWorkCenter> centers = new() { [CenterId] = ManufacturingTestData.WorkCenter(CenterId) };
    private readonly Dictionary<Guid, MfgMachineCenter> machines = [];

    /// <summary>
    /// Queue, setup, run and move time add up per step and the costs follow the hourly rates of the work centre.
    /// </summary>
    [Fact]
    public void Calculate_SerialSteps_AddsTimeAndCosts()
    {
        var routing = ManufacturingTestData.RoutingOf(
            ManufacturingTestData.Operation(10, CenterId, setup: 30m, run: 2m, queue: 10m, move: 5m),
            ManufacturingTestData.Operation(20, CenterId, setup: 0m, run: 1m));

        var result = RoutingLeadTimeCostCalculator.Calculate(routing, centers, machines, quantity: 10m, includeOverlap: false);

        Assert.Equal(75d, result.TotalMinutes);
        Assert.Equal(75d, result.SerialMinutes);
        Assert.Equal(0d, result.OverlapSavingMinutes);
        Assert.Equal(65d, result.Steps[0].DurationMinutes);
        Assert.Equal(0d, result.Steps[0].StartOffsetMinutes);
        Assert.Equal(65d, result.Steps[0].EndOffsetMinutes);
        Assert.Equal(65d, result.Steps[1].StartOffsetMinutes);
        Assert.Equal(75d, result.Steps[1].EndOffsetMinutes);

        // Step 1: setup 30/60*60 + run 20/60*120 + labour 20/60*30 + overhead 50/60*12. Step 2: 20 + 5 + 2.
        Assert.Equal(30m, result.Steps[0].Cost.Setup);
        Assert.Equal(40m, result.Steps[0].Cost.MachineRun);
        Assert.Equal(10m, result.Steps[0].Cost.DirectLabor);
        Assert.Equal(10m, result.Steps[0].Cost.Overhead);
        Assert.Equal(90m, result.Steps[0].Cost.Total);
        Assert.Equal(27m, result.Steps[1].Cost.Total);
        Assert.Equal(117m, result.Cost.Total);
        Assert.Equal(11.7m, result.UnitCost);
        Assert.Equal(0m, result.Cost.Material);
    }

    /// <summary>
    /// With a transfer batch the next step starts before the previous one is complete and the lead time shrinks.
    /// </summary>
    [Fact]
    public void Calculate_WithOverlap_StartsTheNextStepOnTheFirstBatch()
    {
        var routing = ManufacturingTestData.RoutingOf(
            ManufacturingTestData.Operation(10, CenterId, setup: 30m, run: 2m, queue: 10m, move: 5m, overlap: 5m),
            ManufacturingTestData.Operation(20, CenterId, setup: 0m, run: 1m));

        var overlapped = RoutingLeadTimeCostCalculator.Calculate(routing, centers, machines, quantity: 10m, includeOverlap: true);
        var serial = RoutingLeadTimeCostCalculator.Calculate(routing, centers, machines, quantity: 10m, includeOverlap: false);

        Assert.Equal(70d, overlapped.TotalMinutes);
        Assert.Equal(75d, overlapped.SerialMinutes);
        Assert.Equal(5d, overlapped.OverlapSavingMinutes);
        Assert.True(overlapped.Steps[0].OverlapApplied);
        Assert.False(overlapped.Steps[1].OverlapApplied);
        Assert.Equal(55d, overlapped.Steps[1].StartOffsetMinutes);
        Assert.Equal(70d, overlapped.Steps[1].EndOffsetMinutes);
        Assert.Equal(75d, serial.TotalMinutes);
        Assert.False(serial.Steps[0].OverlapApplied);
    }

    /// <summary>
    /// A transfer batch that is not smaller than the quantity gives no overlap.
    /// </summary>
    [Fact]
    public void Calculate_BatchNotSmallerThanQuantity_DoesNotOverlap()
    {
        var routing = ManufacturingTestData.RoutingOf(
            ManufacturingTestData.Operation(10, CenterId, setup: 0m, run: 1m, overlap: 10m),
            ManufacturingTestData.Operation(20, CenterId, setup: 0m, run: 1m));

        var result = RoutingLeadTimeCostCalculator.Calculate(routing, centers, machines, quantity: 10m);

        Assert.Equal(20d, result.TotalMinutes);
        Assert.False(result.Steps[0].OverlapApplied);
    }

    /// <summary>
    /// Parallel units and the efficiency percentage divide the run time; the machine overrides the work centre.
    /// </summary>
    [Fact]
    public void Calculate_ParallelUnitsAndEfficiency_ScaleTheRunTime()
    {
        var machineId = ManufacturingTestData.Machine;
        machines[machineId] = new MfgMachineCenter
        {
            Id = machineId,
            WorkCenterId = CenterId,
            ParallelCapacityUnits = 2,
            EfficiencyFactorPercent = 50m,
            HourlyMachineRunCostRateOverride = 60m,
        };
        var operation = ManufacturingTestData.Operation(10, CenterId, setup: 0m, run: 3m);
        operation.MachineCenterId = machineId;

        var result = RoutingLeadTimeCostCalculator.Calculate(ManufacturingTestData.RoutingOf(operation), centers, machines, quantity: 10m);

        // 3 min x 10 units / (2 parallel x 50 %) = 30 minutes; machine rate override 60/h gives 30.
        Assert.Equal(30d, result.Steps[0].RunMinutes);
        Assert.Equal(30m, result.Steps[0].Cost.MachineRun);
    }

    /// <summary>
    /// The queue and move time default to the work centre values when the operation has none.
    /// </summary>
    [Fact]
    public void Calculate_QueueAndMoveFallBackToTheWorkCentre()
    {
        centers[CenterId].DefaultQueueWaitMinutes = 7m;
        centers[CenterId].DefaultMoveTimeMinutes = 3m;

        var result = RoutingLeadTimeCostCalculator.Calculate(
            ManufacturingTestData.RoutingOf(ManufacturingTestData.Operation(10, CenterId, setup: 0m, run: 1m)),
            centers,
            machines,
            quantity: 1m);

        Assert.Equal(7d, result.Steps[0].QueueMinutes);
        Assert.Equal(3d, result.Steps[0].MoveMinutes);
        Assert.Equal(11d, result.TotalMinutes);
    }

    /// <summary>
    /// A subcontracted step costs the unit price per piece and nothing else.
    /// </summary>
    [Fact]
    public void Calculate_Subcontracted_CostsOnlyTheSubcontractPrice()
    {
        var operation = ManufacturingTestData.Operation(10, CenterId, setup: 15m, run: 1m);
        operation.OperationType = "external_subcontracting";
        operation.SubcontractUnitCost = 3m;

        var result = RoutingLeadTimeCostCalculator.Calculate(ManufacturingTestData.RoutingOf(operation), centers, machines, quantity: 10m);

        Assert.Equal(30m, result.Cost.Subcontracting);
        Assert.Equal(30m, result.Cost.Total);
        Assert.Equal(0m, result.Cost.MachineRun);
        Assert.Equal(0m, result.Cost.DirectLabor);
    }

    /// <summary>
    /// Manual assembly has no machine run cost, and the labour is multiplied by the number of operators.
    /// </summary>
    [Fact]
    public void Calculate_ManualAssembly_HasNoMachineCostAndCountsOperators()
    {
        var operation = ManufacturingTestData.Operation(10, CenterId, setup: 0m, run: 6m);
        operation.OperationType = "manual_assembly";
        operation.RequiredOperators = 2;

        var result = RoutingLeadTimeCostCalculator.Calculate(ManufacturingTestData.RoutingOf(operation), centers, machines, quantity: 10m);

        // 60 labour minutes = 1 h x 30 x 2 operators.
        Assert.Equal(0m, result.Cost.MachineRun);
        Assert.Equal(60m, result.Cost.DirectLabor);
    }

    /// <summary>
    /// A labour time of zero falls back to the run time.
    /// </summary>
    [Fact]
    public void Calculate_LabourTimeDefaultsToRunTime()
    {
        var operation = ManufacturingTestData.Operation(10, CenterId, setup: 0m, run: 6m);
        operation.LaborTimeMinutesPerUnit = 3m;
        var explicitLabour = RoutingLeadTimeCostCalculator.Calculate(ManufacturingTestData.RoutingOf(operation), centers, machines, quantity: 10m);
        operation.LaborTimeMinutesPerUnit = 0m;
        var defaulted = RoutingLeadTimeCostCalculator.Calculate(ManufacturingTestData.RoutingOf(operation), centers, machines, quantity: 10m);

        Assert.Equal(15m, explicitLabour.Cost.DirectLabor);
        Assert.Equal(30m, defaulted.Cost.DirectLabor);
    }

    /// <summary>
    /// The steps are processed in sequence order whatever the order they were loaded in.
    /// </summary>
    [Fact]
    public void Calculate_OrdersTheStepsBySequence()
    {
        var routing = ManufacturingTestData.RoutingOf(
            ManufacturingTestData.Operation(20, CenterId, setup: 0m, run: 2m),
            ManufacturingTestData.Operation(10, CenterId, setup: 0m, run: 1m));

        var result = RoutingLeadTimeCostCalculator.Calculate(routing, centers, machines, quantity: 1m);

        Assert.Equal([10, 20], result.Steps.Select(s => s.Sequence));
        Assert.Equal(1d, result.Steps[0].EndOffsetMinutes);
    }

    /// <summary>
    /// A non-positive quantity, an empty routing and an unknown work centre are rejected.
    /// </summary>
    [Fact]
    public void Calculate_InvalidInput_Throws()
    {
        var routing = ManufacturingTestData.RoutingOf(ManufacturingTestData.Operation(10, CenterId, setup: 0m, run: 1m));

        _ = Assert.Throws<ArgumentOutOfRangeException>(() => RoutingLeadTimeCostCalculator.Calculate(routing, centers, machines, quantity: 0m));
        _ = Assert.Throws<InvalidOperationException>(() => RoutingLeadTimeCostCalculator.Calculate(ManufacturingTestData.RoutingOf(), centers, machines, quantity: 1m));
        _ = Assert.Throws<InvalidOperationException>(() => RoutingLeadTimeCostCalculator.Calculate(routing, new Dictionary<Guid, MfgWorkCenter>(), machines, quantity: 1m));
    }
}
