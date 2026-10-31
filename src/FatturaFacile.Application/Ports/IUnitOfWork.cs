// <copyright file="IUnitOfWork.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Ports;

/// <summary>
/// Encapsulates a transactional unit of work, ensuring atomic persistence of changes across multiple repositories.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Commits all pending entity changes, additions, and deletions made within the current transaction scope.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to observe during the commit operation.</param>
    /// <returns>A task representing the operation, returning the total number of state entries written to the underlying database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
