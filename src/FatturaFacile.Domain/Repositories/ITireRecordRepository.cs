// <copyright file="ITireRecordRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Automotive;

namespace FatturaFacile.Domain.Repositories;

/// <summary>
/// Exposes specialized automotive persistence and query operations for <see cref="TireHotelRecord"/> entities tracking seasonal tire storage and custody.
/// </summary>
public interface ITireRecordRepository : IRepository<TireHotelRecord, Guid>
{

    /// <summary>
    /// Asynchronously retrieves all active seasonal tire storage records currently deposited in the warehouse for a specific customer vehicle.
    /// </summary>
    /// <param name="vehicleId">The unique identifier of the vehicle whose deposited tire sets are being queried.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous query.</param>
    /// <returns>A task representing the asynchronous query; contains a read-only list of active in-storage <see cref="TireHotelRecord"/> entities.</returns>
    public Task<IReadOnlyList<TireHotelRecord>> GetInStorageByVehicleAsync(
        Guid              vehicleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves all active seasonal tire sets currently deposited and stored across all warehouse locations for the active tenant.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous query.</param>
    /// <returns>A task representing the asynchronous query; contains a read-only list of all currently stored <see cref="TireHotelRecord"/> entities.</returns>
    public Task<IReadOnlyList<TireHotelRecord>> GetAllInStorageAsync(CancellationToken cancellationToken = default);

}
