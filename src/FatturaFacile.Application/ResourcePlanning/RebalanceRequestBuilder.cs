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
    /// <exception cref="ArgumentOutOfRangeException">The strategy is not a known one.</exception>
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
            ? new RebalanceRequest(RebalanceStrategy.Shift, draft.EmployeeId, start, EndDate: null, SplitDate: null, ShiftDays: null, AllocatedHoursPerDay: null)
            : new RebalanceRequest(RebalanceStrategy.Shift, draft.EmployeeId, StartDate: null, EndDate: null, SplitDate: null, draft.ShiftDays, AllocatedHoursPerDay: null);

        return Succeed(request);
    }

    private static RebalanceBuildResult Reassign(ResourceAllocation allocation, RebalanceDraft draft) =>
        draft.EmployeeId is { } employee && employee != allocation.EmployeeId
            ? Succeed(new RebalanceRequest(RebalanceStrategy.Reassign, employee, StartDate: null, EndDate: null, SplitDate: null, ShiftDays: null, AllocatedHoursPerDay: null))
            : Fail(ReassignNeedsColleague);

    private static RebalanceBuildResult Split(ResourceAllocation allocation, RebalanceDraft draft)
    {
        if (draft.SplitDate is not { } split || split <= allocation.StartDate || split > allocation.EndDate)
        {
            return Fail(SplitDateOutside);
        }

        var shift = draft.ShiftDays is 0 ? null : draft.ShiftDays;

        return Succeed(new RebalanceRequest(RebalanceStrategy.Split, draft.EmployeeId, StartDate: null, EndDate: null, split, shift, AllocatedHoursPerDay: null));
    }

    private static RebalanceBuildResult Resize(ResourceAllocation allocation, RebalanceDraft draft)
    {
        var start = draft.StartDate is { } s && s != allocation.StartDate ? s : (DateOnly?)null;
        var end = draft.EndDate is { } e && e != allocation.EndDate ? e : (DateOnly?)null;
        var hours = draft.AllocatedHoursPerDay is { } h && h != allocation.AllocatedHoursPerDay ? h : (decimal?)null;

        var error = start is null && end is null && hours is null
            ? ResizeNeedsChange
            : InvalidHoursCode(hours) ?? InvalidPeriodCode(allocation, start, end);

        return error is null
            ? Succeed(new RebalanceRequest(RebalanceStrategy.Resize, EmployeeId: null, start, end, SplitDate: null, ShiftDays: null, hours))
            : Fail(error);
    }

    private static string? InvalidHoursCode(decimal? hours) => hours is { } value && (value <= 0m || value > HoursPerDayMax) ? InvalidHours : null;

    private static string? InvalidPeriodCode(ResourceAllocation allocation, DateOnly? start, DateOnly? end) =>
        (end ?? allocation.EndDate) < (start ?? allocation.StartDate) ? InvalidPeriod : null;

    private static RebalanceBuildResult Succeed(RebalanceRequest request) => new(request, ErrorCode: null);

    private static RebalanceBuildResult Fail(string code) => new(Request: null, code);
}
