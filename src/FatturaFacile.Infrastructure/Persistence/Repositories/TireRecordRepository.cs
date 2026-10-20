using FatturaFacile.Domain.Entities.Automotive;
using FatturaFacile.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

public sealed class TireRecordRepository(FatturaFacileDbContext dbContext) : Repository<TireHotelRecord, Guid>(dbContext), ITireRecordRepository
{
    public async Task<IReadOnlyList<TireHotelRecord>> GetInStorageByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default) => await DbSet.Where(t => t.VehicleId == vehicleId && t.Status == "InStorage").ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TireHotelRecord>> GetAllInStorageAsync(CancellationToken cancellationToken = default) => await DbSet.Where(t => t.Status == "InStorage").ToListAsync(cancellationToken);
}