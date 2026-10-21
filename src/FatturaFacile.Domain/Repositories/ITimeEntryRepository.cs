// <copyright file="ITimeEntryRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.TimeTracking;

namespace FatturaFacile.Domain.Repositories;

/// <summary>
/// Defines the contract for time entry repository.
/// </summary>
public interface ITimeEntryRepository : IRepository<TimeEntry, Guid>
{
    /// <summary>
    /// Get Entries By Employee And Date Range Async.
    /// </summary>
    /// <param name="employeeId">The employee id.</param>
    /// <param name="startUtc">The start utc.</param>
    /// <param name="endUtc">The end utc.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<IReadOnlyList<TimeEntry>> GetEntriesByEmployeeAndDateRangeAsync(
        Guid employeeId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Active Clock In Async.
    /// </summary>
    /// <param name="employeeId">The employee id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<TimeEntry?> GetActiveClockInAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);
}
