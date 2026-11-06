// <copyright file="ClockStatusReader.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Features.TimeTracking.Commands;

/// <summary>
/// Computes the clock status of an employee from today's events.
/// </summary>
public static class ClockStatusReader
{
    /// <summary>
    /// The state of an employee not at work.
    /// </summary>
    public const string Off = "Off";

    /// <summary>
    /// The state of an employee at work.
    /// </summary>
    public const string Working = "Working";

    /// <summary>
    /// The state of an employee on break.
    /// </summary>
    public const string OnBreak = "OnBreak";

    /// <summary>
    /// Reads the status of an employee.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="employee">The employee.</param>
    /// <param name="now">The current time.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The status.</returns>
    public static Task<ClockStatusDto> ReadAsync(IUnitOfWork unitOfWork, HrEmployee employee, DateTime now, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(employee);

        return ReadInternalAsync(unitOfWork, employee, now, cancellationToken);
    }

    private static int Today(DateTime start, DateTime end, DateTime now) =>
        SwissWorkingTimeRules.OverlapMinutes(start, end, now.Date, now.Date.AddDays(1));

    private static async Task<ClockStatusDto> ReadInternalAsync(IUnitOfWork unitOfWork, HrEmployee employee, DateTime now, CancellationToken cancellationToken)
    {
        var since = now.Date.AddDays(-1);
        var employeeId = employee.Id;
        var events = await unitOfWork.Repository<TimeClockEvent>()
            .FindAsync(e => e.EmployeeId == employeeId && e.Timestamp >= since && e.DeletedAt == null, cancellationToken)
            .ConfigureAwait(false);
        var ordered = events.OrderBy(static e => e.Timestamp).ToList();
        var last = ordered.LastOrDefault();
        var state = Off;

        if (TimeEventType.StartsWork(last?.EventType))
        {
            state = Working;
        }
        else if (string.Equals(last?.EventType, TimeEventType.BreakStart, StringComparison.Ordinal))
        {
            state = OnBreak;
        }

        var worked = 0;
        DateTime? open = null;

        foreach (var clock in ordered)
        {
            if (TimeEventType.StartsWork(clock.EventType))
            {
                open = clock.Timestamp;
            }
            else if (open is { } start)
            {
                worked += Today(start, clock.Timestamp, now);
                open = null;
            }
        }

        if (open is { } running)
        {
            worked += Today(running, now, now);
        }

        return new ClockStatusDto(employee.Id, employee.FullName, state, last?.Timestamp, worked);
    }
}
