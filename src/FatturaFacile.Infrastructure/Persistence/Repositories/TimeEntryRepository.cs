using FatturaFacile.Domain.Entities.TimeTracking;
using FatturaFacile.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

public sealed class TimeEntryRepository(FatturaFacileDbContext dbContext) : Repository<TimeEntry, Guid>(dbContext), ITimeEntryRepository
{
    public async Task<IReadOnlyList<TimeEntry>> GetEntriesByEmployeeAndDateRangeAsync(Guid employeeId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.EmployeeId == employeeId && t.ClockInUtc >= startUtc && t.ClockInUtc <= endUtc)
            .OrderByDescending(t => t.ClockInUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<TimeEntry?> GetActiveClockInAsync(Guid employeeId, CancellationToken cancellationToken = default) => DbSet.FirstOrDefaultAsync(t => t.EmployeeId == employeeId && !t.ClockOutUtc.HasValue, cancellationToken);
}