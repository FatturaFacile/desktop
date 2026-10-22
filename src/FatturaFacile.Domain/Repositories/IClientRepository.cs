// <copyright file="IClientRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Crm;

namespace FatturaFacile.Domain.Repositories;

/// <summary>
/// Exposes specialized persistence and query operations for <see cref="Client"/> aggregates across customer relationships and billing profiles.
/// </summary>
public interface IClientRepository : IRepository<Client, Guid>
{
    /// <summary>
    /// Asynchronously retrieves a client matching an Italian or European VAT number (Partita IVA) or fiscal code (Codice Fiscale).
    /// </summary>
    /// <param name="code">The VAT number (Partita IVA) or personal/corporate fiscal code (Codice Fiscale) to look up.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous query.</param>
    /// <returns>A task representing the asynchronous lookup; contains the matching <see cref="Client"/> if found, or <see langword="null"/> otherwise.</returns>
    public Task<Client?> GetByVatOrFiscalCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves all active, non-archived clients available within the active tenant partition.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous query.</param>
    /// <returns>A task representing the asynchronous retrieval; contains a read-only list of active <see cref="Client"/> aggregate roots.</returns>
    public Task<IReadOnlyList<Client>> GetActiveClientsAsync(
        CancellationToken cancellationToken = default);
}
