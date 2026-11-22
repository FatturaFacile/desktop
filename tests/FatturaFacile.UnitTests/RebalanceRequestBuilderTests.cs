// <copyright file="RebalanceRequestBuilderTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Application.ResourcePlanning;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the checks of a rebalance draft against its allocation (period 16 to 27 November, Anna, 6 hours per day).
/// </summary>
public sealed class RebalanceRequestBuilderTests
{
    private static readonly ResourceAllocation Allocation = ResourcePlanningTestData.Allocation(ResourcePlanningTestData.AllocationOne, ResourceBookingType.HardCommitted, ResourcePlanningTestData.ProjectA);

    /// <summary>
    /// A shift by days keeps only the days (and the colleague when dragged to one).
    /// </summary>
    [Fact]
    public void Shift_ByDays_SendsOnlyTheDays()
    {
        var result = RebalanceRequestBuilder.Build(Allocation, Draft(RebalanceStrategy.Shift) with { ShiftDays = 7, EmployeeId = ResourcePlanningTestData.Bruno, StartDate = null });

        Assert.True(result.IsValid);
        Assert.Equal(new RebalanceRequest(RebalanceStrategy.Shift, ResourcePlanningTestData.Bruno, null, null, null, 7, null), result.Request);
    }

    /// <summary>
    /// A shift to a start day sends the start day and no days.
    /// </summary>
    [Fact]
    public void Shift_ToStartDay_SendsTheStartDay()
    {
        var result = RebalanceRequestBuilder.Build(Allocation, Draft(RebalanceStrategy.Shift) with { StartDate = new DateOnly(2026, 11, 23), ShiftDays = 3 });

        Assert.Equal(new RebalanceRequest(RebalanceStrategy.Shift, null, new DateOnly(2026, 11, 23), null, null, null, null), result.Request);
    }

    /// <summary>
    /// A shift without a start day or with zero days is refused.
    /// </summary>
    /// <param name="days">The days.</param>
    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public void Shift_WithoutTarget_IsRefused(int? days)
    {
        var result = RebalanceRequestBuilder.Build(Allocation, Draft(RebalanceStrategy.Shift) with { ShiftDays = days });

        Assert.False(result.IsValid);
        Assert.Equal(RebalanceRequestBuilder.ShiftNeedsTarget, result.ErrorCode);
    }

    /// <summary>
    /// A reassignment needs a colleague other than the current assignee.
    /// </summary>
    [Fact]
    public void Reassign_NeedsAnotherColleague()
    {
        var valid = RebalanceRequestBuilder.Build(Allocation, Draft(RebalanceStrategy.Reassign) with { EmployeeId = ResourcePlanningTestData.Bruno });
        var none = RebalanceRequestBuilder.Build(Allocation, Draft(RebalanceStrategy.Reassign));
        var same = RebalanceRequestBuilder.Build(Allocation, Draft(RebalanceStrategy.Reassign) with { EmployeeId = ResourcePlanningTestData.Anna });

        Assert.Equal(new RebalanceRequest(RebalanceStrategy.Reassign, ResourcePlanningTestData.Bruno, null, null, null, null, null), valid.Request);
        Assert.Equal(RebalanceRequestBuilder.ReassignNeedsColleague, none.ErrorCode);
        Assert.Equal(RebalanceRequestBuilder.ReassignNeedsColleague, same.ErrorCode);
    }

    /// <summary>
    /// A split needs a day after the start and no later than the end; the second part may go to a colleague.
    /// </summary>
    [Fact]
    public void Split_NeedsADayInsideTheAllocation()
    {
        var valid = RebalanceRequestBuilder.Build(Allocation, Draft(RebalanceStrategy.Split) with { SplitDate = new DateOnly(2026, 11, 23), EmployeeId = ResourcePlanningTestData.Bruno, ShiftDays = 0 });
        var onStart = RebalanceRequestBuilder.Build(Allocation, Draft(RebalanceStrategy.Split) with { SplitDate = new DateOnly(2026, 11, 16) });
        var afterEnd = RebalanceRequestBuilder.Build(Allocation, Draft(RebalanceStrategy.Split) with { SplitDate = new DateOnly(2026, 11, 28) });
        var missing = RebalanceRequestBuilder.Build(Allocation, Draft(RebalanceStrategy.Split));

        Assert.Equal(new RebalanceRequest(RebalanceStrategy.Split, ResourcePlanningTestData.Bruno, null, null, new DateOnly(2026, 11, 23), null, null), valid.Request);
        Assert.Equal(RebalanceRequestBuilder.SplitDateOutside, onStart.ErrorCode);
        Assert.Equal(RebalanceRequestBuilder.SplitDateOutside, afterEnd.ErrorCode);
        Assert.Equal(RebalanceRequestBuilder.SplitDateOutside, missing.ErrorCode);
        Assert.True(RebalanceRequestBuilder.Build(Allocation, Draft(RebalanceStrategy.Split) with { SplitDate = new DateOnly(2026, 11, 27) }).IsValid);
    }

    /// <summary>
    /// A resize sends only what changed.
    /// </summary>
    [Fact]
    public void Resize_SendsOnlyTheChanges()
    {
        var result = RebalanceRequestBuilder.Build(Allocation, Draft(RebalanceStrategy.Resize) with { StartDate = Allocation.StartDate, EndDate = new DateOnly(2026, 12, 4), AllocatedHoursPerDay = 4.5m });

        Assert.Equal(new RebalanceRequest(RebalanceStrategy.Resize, null, null, new DateOnly(2026, 12, 4), null, null, 4.5m), result.Request);
    }

    /// <summary>
    /// A resize that changes nothing, ends before it starts or uses impossible hours is refused.
    /// </summary>
    [Fact]
    public void Resize_InvalidChanges_AreRefused()
    {
        var unchanged = RebalanceRequestBuilder.Build(Allocation, Draft(RebalanceStrategy.Resize) with { StartDate = Allocation.StartDate, EndDate = Allocation.EndDate, AllocatedHoursPerDay = Allocation.AllocatedHoursPerDay });
        var backwards = RebalanceRequestBuilder.Build(Allocation, Draft(RebalanceStrategy.Resize) with { EndDate = new DateOnly(2026, 11, 10) });
        var tooMany = RebalanceRequestBuilder.Build(Allocation, Draft(RebalanceStrategy.Resize) with { AllocatedHoursPerDay = 25m });
        var zero = RebalanceRequestBuilder.Build(Allocation, Draft(RebalanceStrategy.Resize) with { AllocatedHoursPerDay = 0m });

        Assert.Equal(RebalanceRequestBuilder.ResizeNeedsChange, unchanged.ErrorCode);
        Assert.Equal(RebalanceRequestBuilder.InvalidPeriod, backwards.ErrorCode);
        Assert.Equal(RebalanceRequestBuilder.InvalidHours, tooMany.ErrorCode);
        Assert.Equal(RebalanceRequestBuilder.InvalidHours, zero.ErrorCode);
    }

    private static RebalanceDraft Draft(RebalanceStrategy strategy) => new(strategy, null, null, null, null, null, null);
}
