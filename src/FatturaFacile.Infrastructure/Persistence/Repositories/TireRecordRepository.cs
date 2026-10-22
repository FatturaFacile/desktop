// <copyright file="TireRecordRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Automotive;
using FatturaFacile.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository managing automotive tire hotel storage check-ins, tread depth logs, and rack positions.
/// </summary>
/// <param name="dbContext">The application database context.</param>
public sealed class TireRecordRepository(FatturaFacileDbContext dbContext)
    : Repository<TireHotelRecord, Guid>(dbContext: dbContext), ITireRecordRepository
{

    /// <summary>
    /// Retrieves all tire hotel sets currently held in storage for the specified vehicle.
    /// </summary>
    /// <param name="vehicleId">The vehicle identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only collection of <see cref="TireHotelRecord"/> entities with "InStorage" status.</returns>
    public async Task<IReadOnlyList<TireHotelRecord>> GetInStorageByVehicleAsync(
        Guid              vehicleId,
        CancellationToken cancellationToken = default) => await DbSet
                                                               .Where(predicate: t => t.VehicleId == vehicleId &&
                                                                                      t.Status    == "InStorage")
                                                               .ToListAsync(cancellationToken: cancellationToken);

    /// <summary>
    /// Retrieves all tire sets across all vehicles currently held in the hotel storage racks.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only collection of all stored tire hotel records.</returns>
    public async Task<IReadOnlyList<TireHotelRecord>>
        GetAllInStorageAsync(CancellationToken cancellationToken = default) =>
        await DbSet.Where(predicate: t => t.Status == "InStorage").ToListAsync(cancellationToken: cancellationToken);

}
