using FatturaFacile.Domain.Entities.Automotive;

namespace FatturaFacile.Domain.Repositories;

public interface ITireRecordRepository : IRepository<TireHotelRecord, Guid>
{
    public Task<IReadOnlyList<TireHotelRecord>> GetInStorageByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<TireHotelRecord>> GetAllInStorageAsync(CancellationToken cancellationToken = default);
}