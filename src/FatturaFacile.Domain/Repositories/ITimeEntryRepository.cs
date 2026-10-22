// <copyright file="ITimeEntryRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.TimeTracking;

namespace FatturaFacile.Domain.Repositories;

/// <summary>
/// Exposes specialized persistence and query operations
/// for employee <see cref="TimeEntry"/> records supporting attendance, clock-in, and payroll tracking.
/// </summary>
public interface ITimeEntryRepository : IRepository<TimeEntry, Guid>
{

    /// <summary>
    /// Asynchronously retrieves all logged time entries for an employee falling within the specified UTC date and time window.
    /// </summary>
    /// <param name="employeeId">The unique identifier of the target employee.</param>
    /// <param name="startUtc">The lower boundary UTC timestamp of the query interval (inclusive).</param>
    /// <param name="endUtc">The upper boundary UTC timestamp of the query interval (inclusive).</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous query.</param>
    /// <returns>A task representing the asynchronous query;
    /// contains a read-only list of <see cref="TimeEntry"/> records within the range.</returns>
    public Task<IReadOnlyList<TimeEntry>> GetEntriesByEmployeeAndDateRangeAsync(
        Guid              employeeId,
        DateTime          startUtc,
        DateTime          endUtc,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the currently open, active clock-in session for the specified employee.
    /// </summary>
    /// <param name="employeeId">The unique identifier of the employee whose active work shift is being queried.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous query.</param>
    /// <returns>A task representing the asynchronous query;
    /// contains the active in-progress <see cref="TimeEntry"/> if clocked in,
    /// or <see langword="null"/> if the employee is currently clocked out.</returns>
    public Task<TimeEntry?> GetActiveClockInAsync(
        Guid              employeeId,
        CancellationToken cancellationToken = default);

}
