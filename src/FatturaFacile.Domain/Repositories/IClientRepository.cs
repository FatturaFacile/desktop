// <copyright file="IClientRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Crm;

namespace FatturaFacile.Domain.Repositories;

/// <summary>
/// Defines the contract for client repository.
/// </summary>
public interface IClientRepository : IRepository<Client, Guid>
{
    /// <summary>
    /// Get By Vat Or Fiscal Code Async.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<Client?> GetByVatOrFiscalCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Active Clients Async.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<IReadOnlyList<Client>> GetActiveClientsAsync(
        CancellationToken cancellationToken = default);
}
