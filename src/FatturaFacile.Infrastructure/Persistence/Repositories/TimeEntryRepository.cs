// <copyright file="TimeEntryRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.TimeTracking;
using FatturaFacile.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

/// <summary>
/// Represents time entry repository.
/// </summary>
/// <param name="dbContext">The db context.</param>
public sealed class TimeEntryRepository(FatturaFacileDbContext dbContext)
    : Repository<TimeEntry, Guid>(dbContext), ITimeEntryRepository
{
    /// <summary>
    /// Get Entries By Employee And Date Range Async.
    /// </summary>
    /// <param name="employeeId">The employee id.</param>
    /// <param name="startUtc">The start utc.</param>
    /// <param name="endUtc">The end utc.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public async Task<IReadOnlyList<TimeEntry>> GetEntriesByEmployeeAndDateRangeAsync(
        Guid employeeId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.EmployeeId == employeeId && t.ClockInUtc >= startUtc && t.ClockInUtc <= endUtc)
            .OrderByDescending(t => t.ClockInUtc)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get Active Clock In Async.
    /// </summary>
    /// <param name="employeeId">The employee id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<TimeEntry?> GetActiveClockInAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(
            t => t.EmployeeId == employeeId && !t.ClockOutUtc.HasValue,
            cancellationToken);
}
