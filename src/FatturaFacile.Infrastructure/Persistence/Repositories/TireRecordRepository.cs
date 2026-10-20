using FatturaFacile.Domain.Entities.Automotive;
using FatturaFacile.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

/// <summary>
/// Represents tire record repository.
/// </summary>
/// <param name="dbContext">The db context.</param>
public sealed class TireRecordRepository(FatturaFacileDbContext dbContext) : Repository<TireHotelRecord, Guid>(dbContext), ITireRecordRepository
{
/// <summary>
/// Get In Storage By Vehicle Async.
/// </summary>
/// <param name="vehicleId">The vehicle id.</param>
/// <param name="cancellationToken">The cancellation token.</param>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public async Task<IReadOnlyList<TireHotelRecord>> GetInStorageByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default) => await DbSet.Where(t => t.VehicleId == vehicleId && t.Status == "InStorage").ToListAsync(cancellationToken);

/// <summary>
/// Get All In Storage Async.
/// </summary>
/// <param name="cancellationToken">The cancellation token.</param>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public async Task<IReadOnlyList<TireHotelRecord>> GetAllInStorageAsync(CancellationToken cancellationToken = default) => await DbSet.Where(t => t.Status == "InStorage").ToListAsync(cancellationToken);
}
