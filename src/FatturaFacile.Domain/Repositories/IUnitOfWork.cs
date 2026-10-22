// <copyright file="IUnitOfWork.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Repositories;

/// <summary>
/// Encapsulates the transactional boundary and coordinates the persistence of changes across aggregate roots and repositories.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Asynchronously commits all pending database changes and dispatches recorded domain events within the transactional boundary.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the commit transaction.</param>
    /// <returns>A task representing the asynchronous save operation; the task result contains the number of state entries written to underlying storage.</returns>
    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
