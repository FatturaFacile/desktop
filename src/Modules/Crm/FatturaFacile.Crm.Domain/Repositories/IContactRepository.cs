// <copyright file="IContactRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Crm.Domain.Aggregates;
using FatturaFacile.Domain.Shared.Repositories;

namespace FatturaFacile.Crm.Domain.Repositories;

/// <summary>
/// Domain repository contract for persisting and retrieving <see cref="Contact"/> aggregate roots.
/// </summary>
public interface IContactRepository : IRepository
{
    /// <summary>
    /// Retrieves a contact by its unique identifier.
    /// </summary>
    /// <param name="id">The contact identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching <see cref="Contact"/> if found; otherwise, null.</returns>
    public Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new contact to the repository store.
    /// </summary>
    /// <param name="contact">The contact aggregate root instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task AddAsync(Contact contact, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing contact in the repository store.
    /// </summary>
    /// <param name="contact">The modified contact aggregate root instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task UpdateAsync(Contact contact, CancellationToken cancellationToken = default);
}
