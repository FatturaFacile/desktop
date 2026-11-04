// <copyright file="IGeneralLedgerRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Accounting.Domain.Aggregates;
using FatturaFacile.Domain.Shared.Repositories;

namespace FatturaFacile.Accounting.Domain.Repositories;

/// <summary>
/// Domain repository contract for accessing and persisting <see cref="GeneralLedgerEntry"/> aggregate roots.
/// </summary>
public interface IGeneralLedgerRepository : IRepository
{
    /// <summary>
    /// Retrieves a general ledger entry by its unique identifier including its transaction lines.
    /// </summary>
    /// <param name="id">The general ledger entry identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching <see cref="GeneralLedgerEntry"/> if found; otherwise, null.</returns>
    public Task<GeneralLedgerEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new general ledger entry to the repository store.
    /// </summary>
    /// <param name="entry">The general ledger entry instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task AddAsync(GeneralLedgerEntry entry, CancellationToken cancellationToken = default);
}
