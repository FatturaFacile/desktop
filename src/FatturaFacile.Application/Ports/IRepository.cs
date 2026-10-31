// <copyright file="IRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Linq.Expressions;
using FatturaFacile.Domain.Common;

namespace FatturaFacile.Application.Ports;

/// <summary>
/// Generic repository interface defining standard CRUD data access contracts for domain entities.
/// </summary>
/// <typeparam name="T">The domain entity type derived from <see cref="EntityBase"/>.</typeparam>
public interface IRepository<T> where T : EntityBase
{
    /// <summary>
    /// Retrieves a single entity by its primary unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">Cancellation token to observe during query execution.</param>
    /// <returns>A task representing the asynchronous read, containing the entity if found; otherwise, null.</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities matching an optional filtering predicate expression.
    /// </summary>
    /// <param name="predicate">An optional lambda expression specifying filtering conditions.</param>
    /// <param name="cancellationToken">Cancellation token to observe during query execution.</param>
    /// <returns>A task containing a read-only list of matching domain entities.</returns>
    Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously registers a new entity within the data context for persistence.
    /// </summary>
    /// <param name="entity">The entity instance to add.</param>
    /// <param name="cancellationToken">Cancellation token to observe during the operation.</param>
    /// <returns>A task representing the asynchronous persistence operation.</returns>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity's state within the data context.
    /// </summary>
    /// <param name="entity">The entity instance containing updated values.</param>
    /// <param name="cancellationToken">Cancellation token to observe during the operation.</param>
    /// <returns>A task representing the asynchronous persistence operation.</returns>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an existing entity from the data context.
    /// </summary>
    /// <param name="entity">The entity instance to remove.</param>
    /// <param name="cancellationToken">Cancellation token to observe during the operation.</param>
    /// <returns>A task representing the asynchronous persistence operation.</returns>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}
