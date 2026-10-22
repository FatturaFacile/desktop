// <copyright file="IRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Linq.Expressions;
using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Repositories;

/// <summary>
/// Exposes generic asynchronous persistence and querying operations for domain aggregate roots and entities.
/// </summary>
/// <typeparam name="TEntity">The concrete domain entity or aggregate root type managed by this repository.</typeparam>
/// <typeparam name="TId">The type of the primary identifier uniquely distinguishing the entity.</typeparam>
public interface IRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    /// <summary>
    /// Asynchronously retrieves an entity instance matching the specified primary identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous query.</param>
    /// <returns>A task that represents the asynchronous retrieval; contains the matching entity if found, or <see langword="null"/> otherwise.</returns>
    public Task<TEntity?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves all accessible instances of the entity within the current partition context.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous query.</param>
    /// <returns>A task that represents the asynchronous retrieval; contains a read-only list of all retrieved entities.</returns>
    public Task<IReadOnlyList<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously finds all entities satisfying the specified specification or filter expression.
    /// </summary>
    /// <param name="predicate">A strongly typed lambda expression defining the entity evaluation criteria.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous query.</param>
    /// <returns>A task that represents the asynchronous retrieval; contains a read-only list of entities satisfying the predicate.</returns>
    public Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enrolls a newly instantiated entity into the persistence tracking context for insertion upon unit of work commit.
    /// </summary>
    /// <param name="entity">The domain entity instance to insert.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the operation.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    public Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing entity as modified within the persistence tracking context for update upon unit of work commit.
    /// </summary>
    /// <param name="entity">The domain entity instance containing updated state.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the operation.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    public Task UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enrolls an existing entity for deletion or soft removal within the persistence tracking context upon unit of work commit.
    /// </summary>
    /// <param name="entity">The domain entity instance targeted for deletion.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the operation.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    public Task DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);
}
