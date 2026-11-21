// <copyright file="FiniteCapacitySchedulerTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Manufacturing.Scheduling;
using Xunit;

namespace FatturaFacile.UnitTests.Manufacturing;

/// <summary>
/// Tests of the local forward finite-capacity scheduler (Monday to Friday, 08:00 start, daily capacity per work centre).
/// </summary>
public sealed class FiniteCapacitySchedulerTests
{
    private static readonly DateTime Monday = new(2026, 11, 23);
    private static readonly Guid W1 = Guid.NewGuid();
    private static readonly Guid W2 = Guid.NewGuid();

    /// <summary>
    /// An operation starts at 08:00 of the first working day.
    /// </summary>
    [Fact]
    public void Schedule_SingleOperation_StartsAtEight()
    {
        var result = FiniteCapacityScheduler.Schedule([Order("O1", 1, Monday, (W1, 120m))], Centers(), Monday);

        var task = Assert.Single(result.Tasks);
        Assert.Equal(Monday.AddHours(8), task.Start);
        Assert.Equal(Monday.AddHours(10), task.End);
    }

    /// <summary>
    /// A start on a weekend moves to Monday morning.
    /// </summary>
    [Fact]
    public void Schedule_WeekendStart_MovesToMonday()
    {
        var saturday = new DateTime(2026, 11, 21);

        var result = FiniteCapacityScheduler.Schedule([Order("O1", 1, saturday, (W1, 60m))], Centers(), saturday);

        Assert.Equal(Monday.AddHours(8), result.Tasks.Single().Start);
    }

    /// <summary>
    /// Two orders compete for one work centre: the second waits and spills into the next day when the capacity is exhausted.
    /// </summary>
    [Fact]
    public void Schedule_SameWorkCentre_IsFiniteAndSpillsToNextDay()
    {
        var result = FiniteCapacityScheduler.Schedule(
            [Order("O1", 1, Monday, (W1, 300m)), Order("O2", 2, Monday, (W1, 300m))],
            Centers(),
            Monday);

        var second = result.Tasks.Single(t => string.Equals(t.OrderNumber, "O2", StringComparison.Ordinal));
        Assert.Equal(Monday.AddHours(13), second.Start);
        Assert.Equal(Monday.AddDays(1).AddHours(10), second.End);
    }

    /// <summary>
    /// The weekend is skipped when an operation crosses the end of the week.
    /// </summary>
    [Fact]
    public void Schedule_AcrossWeekend_ContinuesOnMonday()
    {
        var friday = new DateTime(2026, 11, 27);

        var result = FiniteCapacityScheduler.Schedule([Order("O1", 1, friday, (W1, 600m))], Centers(), friday);

        var task = result.Tasks.Single();
        Assert.Equal(friday.AddHours(8), task.Start);
        Assert.Equal(new DateTime(2026, 11, 30, 10, 0, 0), task.End);
    }

    /// <summary>
    /// The next operation of an order never starts before the previous one finished, even on another work centre.
    /// </summary>
    [Fact]
    public void Schedule_OperationsOfAnOrder_AreSequential()
    {
        var result = FiniteCapacityScheduler.Schedule([Order("O1", 1, Monday, (W1, 120m), (W2, 60m))], Centers(), Monday);

        var tasks = result.Tasks.OrderBy(t => t.Sequence).ToList();
        Assert.Equal(tasks[0].End, tasks[1].Start);
        Assert.Equal(Monday.AddHours(11), tasks[1].End);
        Assert.Equal(tasks[1].End, result.LastFinish);
    }

    /// <summary>
    /// A lower priority number is dispatched first.
    /// </summary>
    [Fact]
    public void Schedule_Priority_DecidesTheOrder()
    {
        var result = FiniteCapacityScheduler.Schedule(
            [Order("LATE", 5, Monday, (W1, 60m)), Order("URGENT", 1, Monday, (W1, 60m))],
            Centers(),
            Monday);

        Assert.Equal(Monday.AddHours(8), result.Tasks.Single(t => string.Equals(t.OrderNumber, "URGENT", StringComparison.Ordinal)).Start);
        Assert.Equal(Monday.AddHours(9), result.Tasks.Single(t => string.Equals(t.OrderNumber, "LATE", StringComparison.Ordinal)).Start);
    }

    /// <summary>
    /// An operation on a work centre without capacity is reported.
    /// </summary>
    [Fact]
    public void Schedule_UnknownWorkCentre_Throws()
    {
        var error = Assert.Throws<InvalidOperationException>(() => FiniteCapacityScheduler.Schedule([Order("O1", 1, Monday, (Guid.NewGuid(), 60m))], Centers(), Monday));

        Assert.Contains("capacity", error.Message, StringComparison.Ordinal);
    }

    private static WorkCenterCapacity[] Centers() => [new(W1, "W1", 480m), new(W2, "W2", 480m)];

    private static SchedulableOrder Order(string number, int priority, DateTime earliest, params (Guid Center, decimal Minutes)[] operations)
    {
        var sequence = 10;
        List<SchedulableOperation> list = [];
        foreach (var (center, minutes) in operations)
        {
            list.Add(new SchedulableOperation(Guid.NewGuid(), sequence, "Op" + sequence.ToString(CultureInfo.InvariantCulture), center, minutes));
            sequence += 10;
        }

        return new SchedulableOrder(Guid.NewGuid(), number, priority, earliest, list);
    }
}
