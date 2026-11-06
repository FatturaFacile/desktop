// <copyright file="IRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Linq.Expressions;
using FatturaFacile.Domain.Common;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Generic repository interface for querying and persisting domain POCO entities.
/// </summary>
/// <typeparam name="T">The entity type implementing <see cref="IBaseEntity"/>.</typeparam>
public interface IRepository<T>
    where T : class, IBaseEntity
{
    /// <summary>
    /// Gets all entities as an asynchronous read-only queryable.
    /// </summary>
    /// <returns>A queryable representation of the entity collection.</returns>
    public IQueryable<T> Query();

    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching entity, or null if not found.</returns>
    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves entities matching the given predicate.
    /// </summary>
    /// <param name="predicate">Filter predicate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of matching entities.</returns>
    public Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the repository context.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    public Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the repository context.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    public void Update(T entity);

    /// <summary>
    /// Deletes an entity from the repository context.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    public void Delete(T entity);
}
