using FatturaFacile.Domain.Entities.TimeTracking;

namespace FatturaFacile.Domain.Repositories;

public interface ITimeEntryRepository : IRepository<TimeEntry, Guid>
{
    public Task<IReadOnlyList<TimeEntry>> GetEntriesByEmployeeAndDateRangeAsync(Guid employeeId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default);
    public Task<TimeEntry?> GetActiveClockInAsync(Guid employeeId, CancellationToken cancellationToken = default);
}