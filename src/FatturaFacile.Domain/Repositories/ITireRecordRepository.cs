// <copyright file="ITireRecordRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Automotive;

namespace FatturaFacile.Domain.Repositories;

/// <summary>
/// Defines the contract for tire record repository.
/// </summary>
public interface ITireRecordRepository : IRepository<TireHotelRecord, Guid>
{
    /// <summary>
    /// Get In Storage By Vehicle Async.
    /// </summary>
    /// <param name="vehicleId">The vehicle id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<IReadOnlyList<TireHotelRecord>> GetInStorageByVehicleAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get All In Storage Async.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<IReadOnlyList<TireHotelRecord>> GetAllInStorageAsync(
        CancellationToken cancellationToken = default);
}
