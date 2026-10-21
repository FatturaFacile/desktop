// <copyright file="IRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Linq.Expressions;
using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Repositories;

/// <summary>
/// Defines the contract for repository.
/// </summary>
/// <typeparam name="TEntity">The type of the t entity.</typeparam>
/// <typeparam name="TId">The type of the t id.</typeparam>
public interface IRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    /// <summary>
    /// Get By Id Async.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<TEntity?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get All Async.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<IReadOnlyList<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find Async.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add Async.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update Async.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete Async.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);
}
