// <copyright file="TimeEntryRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.TimeTracking;
using FatturaFacile.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository managing employee work time recordings, attendance clock-ins, and period summaries.
/// </summary>
/// <param name="dbContext">The application database context.</param>
public sealed class TimeEntryRepository(FatturaFacileDbContext dbContext)
    : Repository<TimeEntry, Guid>(dbContext: dbContext), ITimeEntryRepository
{

    /// <summary>
    /// Retrieves all time recording entries for a specific employee within a UTC date interval.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="startUtc">The beginning UTC date and time.</param>
    /// <param name="endUtc">The ending UTC date and time.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only collection of time entries ordered descending by clock-in time.</returns>
    public async Task<IReadOnlyList<TimeEntry>> GetEntriesByEmployeeAndDateRangeAsync(
        Guid              employeeId,
        DateTime          startUtc,
        DateTime          endUtc,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
                    .Where(predicate: t => t.EmployeeId == employeeId && t.ClockInUtc >= startUtc && t.ClockInUtc <= endUtc)
                    .OrderByDescending(keySelector: t => t.ClockInUtc)
                    .ToListAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Finds the currently open (uncompleted) clock-in record for an employee without a recorded clock-out.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The active <see cref="TimeEntry"/> if the employee is currently clocked in; otherwise, <see langword="null"/>.</returns>
    public Task<TimeEntry?> GetActiveClockInAsync(
        Guid              employeeId,
        CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(
                                  predicate: t => t.EmployeeId == employeeId && !t.ClockOutUtc.HasValue,
                                  cancellationToken: cancellationToken);

}
