// <copyright file="IUnitOfWork.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Repositories;

/// <summary>
/// Defines the contract for unit of work.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Save Changes Async.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
