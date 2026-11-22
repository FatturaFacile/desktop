// <copyright file="RebalanceRequestBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// Checks a rebalance draft against its allocation before it is sent and builds the request with only the parameters of the strategy.
/// </summary>
public static class RebalanceRequestBuilder
{
    /// <summary>
    /// The problem code when the shift has neither a new start day nor a number of days.
    /// </summary>
    public const string ShiftNeedsTarget = "shift_needs_target";

    /// <summary>
    /// The problem code when the reassignment names no colleague or the same employee.
    /// </summary>
    public const string ReassignNeedsColleague = "reassign_needs_colleague";

    /// <summary>
    /// The problem code when the split day is missing or outside the allocation.
    /// </summary>
    public const string SplitDateOutside = "split_date_outside";

    /// <summary>
    /// The problem code when the resize changes nothing.
    /// </summary>
    public const string ResizeNeedsChange = "resize_needs_change";

    /// <summary>
    /// The problem code when the resized period ends before it starts.
    /// </summary>
    public const string InvalidPeriod = "invalid_period";

    /// <summary>
    /// The problem code when the hours per day are not between 0 and 24.
    /// </summary>
    public const string InvalidHours = "invalid_hours";

    private const decimal HoursPerDayMax = 24m;

    /// <summary>
    /// Builds the request of a rebalance.
    /// </summary>
    /// <param name="allocation">The allocation to rebalance.</param>
    /// <param name="draft">What the planner entered.</param>
    /// <returns>The request or the first problem.</returns>
    public static RebalanceBuildResult Build(ResourceAllocation allocation, RebalanceDraft draft)
    {
        ArgumentNullException.ThrowIfNull(allocation);
        ArgumentNullException.ThrowIfNull(draft);

        return draft.Strategy switch
        {
            RebalanceStrategy.Shift => Shift(draft),
            RebalanceStrategy.Reassign => Reassign(allocation, draft),
            RebalanceStrategy.Split => Split(allocation, draft),
            RebalanceStrategy.Resize => Resize(allocation, draft),
            _ => throw new ArgumentOutOfRangeException(nameof(draft), draft.Strategy, "Unknown rebalance strategy."),
        };
    }

    private static RebalanceBuildResult Shift(RebalanceDraft draft)
    {
        if (draft.StartDate is null && draft.ShiftDays is null or 0)
        {
            return Fail(ShiftNeedsTarget);
        }

        var request = draft.StartDate is { } start
            ? new RebalanceRequest(RebalanceStrategy.Shift, draft.EmployeeId, start, null, null, null, null)
            : new RebalanceRequest(RebalanceStrategy.Shift, draft.EmployeeId, null, null, null, draft.ShiftDays, null);

        return new RebalanceBuildResult(request, null);
    }

    private static RebalanceBuildResult Reassign(ResourceAllocation allocation, RebalanceDraft draft)
    {
        if (draft.EmployeeId is not { } employee || employee == allocation.EmployeeId)
        {
            return Fail(ReassignNeedsColleague);
        }

        return new RebalanceBuildResult(new RebalanceRequest(RebalanceStrategy.Reassign, employee, null, null, null, null, null), null);
    }

    private static RebalanceBuildResult Split(ResourceAllocation allocation, RebalanceDraft draft)
    {
        if (draft.SplitDate is not { } split || split <= allocation.StartDate || split > allocation.EndDate)
        {
            return Fail(SplitDateOutside);
        }

        var shift = draft.ShiftDays is 0 ? null : draft.ShiftDays;

        return new RebalanceBuildResult(new RebalanceRequest(RebalanceStrategy.Split, draft.EmployeeId, null, null, split, shift, null), null);
    }

    private static RebalanceBuildResult Resize(ResourceAllocation allocation, RebalanceDraft draft)
    {
        var start = draft.StartDate is { } s && s != allocation.StartDate ? s : (DateOnly?)null;
        var end = draft.EndDate is { } e && e != allocation.EndDate ? e : (DateOnly?)null;
        var hours = draft.AllocatedHoursPerDay is { } h && h != allocation.AllocatedHoursPerDay ? h : (decimal?)null;

        if (start is null && end is null && hours is null)
        {
            return Fail(ResizeNeedsChange);
        }

        if (hours is { } value && (value <= 0m || value > HoursPerDayMax))
        {
            return Fail(InvalidHours);
        }

        if ((end ?? allocation.EndDate) < (start ?? allocation.StartDate))
        {
            return Fail(InvalidPeriod);
        }

        return new RebalanceBuildResult(new RebalanceRequest(RebalanceStrategy.Resize, null, start, end, null, null, hours), null);
    }

    private static RebalanceBuildResult Fail(string code) => new(null, code);
}
