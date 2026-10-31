// <copyright file="IContactRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Ports;

/// <summary>
/// Defines data access operations for customer, client, and vendor contact records within a tenant.
/// </summary>
public interface IContactRepository : IRepository<Contact>
{
    /// <summary>
    /// Retrieves all registered contact records for the current tenant.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to observe during query execution.</param>
    /// <returns>A read-only collection of tenant contacts.</returns>
    Task<IReadOnlyList<Contact>> GetAllContactsAsync(CancellationToken cancellationToken = default);
}
