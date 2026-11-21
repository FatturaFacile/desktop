// <copyright file="FiniteCapacityScheduler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing.Scheduling;

/// <summary>
/// Local forward finite-capacity scheduler. Orders are dispatched one after the other by priority, then earliest start, then number.
/// Each work centre works Monday to Friday from 08:00 for its daily productive minutes; an operation that does not fit the day continues on the next working day.
/// An operation never starts before the previous operation of its order finished.
/// </summary>
public static class FiniteCapacityScheduler
{
    private static readonly TimeSpan DayStart = TimeSpan.FromHours(8);

    /// <summary>
    /// Schedules the orders against the work centre capacities.
    /// </summary>
    /// <param name="orders">The orders to schedule.</param>
    /// <param name="centers">The work centre capacities.</param>
    /// <param name="horizonStart">The earliest instant any operation may start.</param>
    /// <returns>The scheduled tasks.</returns>
    /// <exception cref="InvalidOperationException">An operation uses a work centre without capacity.</exception>
    public static ScheduleResult Schedule(IEnumerable<SchedulableOrder> orders, IEnumerable<WorkCenterCapacity> centers, DateTime horizonStart)
    {
        ArgumentNullException.ThrowIfNull(orders);
        ArgumentNullException.ThrowIfNull(centers);

        var capacity = centers.ToDictionary(c => c.WorkCenterId);
        Dictionary<Guid, DateTime> cursors = [];
        List<ScheduledTask> tasks = [];

        foreach (var order in orders.OrderBy(o => o.Priority).ThenBy(o => o.EarliestStart).ThenBy(o => o.OrderNumber, StringComparer.Ordinal))
        {
            var previousEnd = order.EarliestStart > horizonStart ? order.EarliestStart : horizonStart;

            foreach (var operation in order.Operations.OrderBy(o => o.Sequence))
            {
                if (!capacity.TryGetValue(operation.WorkCenterId, out var center) || center.DailyMinutes <= 0m)
                {
                    throw new InvalidOperationException($"Work centre {operation.WorkCenterId} of order {order.OrderNumber} has no capacity.");
                }

                var notBefore = cursors.TryGetValue(center.WorkCenterId, out var cursor) && cursor > previousEnd ? cursor : previousEnd;
                var (start, end) = Place(notBefore, operation.Minutes, (double)center.DailyMinutes);
                cursors[center.WorkCenterId] = end;
                previousEnd = end;

                tasks.Add(new ScheduledTask(order.OrderId, order.OrderNumber, operation.OperationId, operation.Sequence, operation.Name, center.WorkCenterId, center.Code, start, end, operation.Minutes));
            }
        }

        return new ScheduleResult(tasks, tasks.Count == 0 ? null : tasks.Max(t => t.End));
    }

    private static (DateTime Start, DateTime End) Place(DateTime notBefore, decimal minutes, double dailyMinutes)
    {
        var moment = NextWorkingMoment(notBefore, dailyMinutes);
        var start = moment;
        var remaining = (double)minutes;

        while (true)
        {
            var available = dailyMinutes - (moment - moment.Date.Add(DayStart)).TotalMinutes;
            var take = Math.Min(remaining, available);
            moment = moment.AddMinutes(take);
            remaining -= take;
            if (remaining <= 0.0000001)
            {
                return (start, moment);
            }

            moment = NextWorkingDayStart(moment.Date.AddDays(1));
        }
    }

    private static DateTime NextWorkingMoment(DateTime from, double dailyMinutes)
    {
        if (IsWeekend(from.Date))
        {
            return NextWorkingDayStart(from.Date);
        }

        var dayStart = from.Date.Add(DayStart);
        var moment = from < dayStart ? dayStart : from;
        return (moment - dayStart).TotalMinutes >= dailyMinutes ? NextWorkingDayStart(from.Date.AddDays(1)) : moment;
    }

    private static bool IsWeekend(DateTime date) => date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    private static DateTime NextWorkingDayStart(DateTime date)
    {
        var day = date.Date;
        while (IsWeekend(day))
        {
            day = day.AddDays(1);
        }

        return day.Add(DayStart);
    }
}
