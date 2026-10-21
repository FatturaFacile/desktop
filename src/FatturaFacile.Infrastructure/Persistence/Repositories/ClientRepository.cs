// <copyright file="ClientRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Crm;
using FatturaFacile.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

/// <summary>
/// Represents client repository.
/// </summary>
/// <param name="dbContext">The db context.</param>
public sealed class ClientRepository(FatturaFacileDbContext dbContext)
    : Repository<Client, Guid>(dbContext), IClientRepository
{
    /// <summary>
    /// Get By Vat Or Fiscal Code Async.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<Client?> GetByVatOrFiscalCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(
            c =>
                (c.FiscalCode != null && c.FiscalCode.Value == code) ||
                (c.VatNumber != null && c.VatNumber.Value == code) ||
                c.SwissUid == code,
            cancellationToken);
    }

    /// <summary>
    /// Get Active Clients Async.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public async Task<IReadOnlyList<Client>> GetActiveClientsAsync(
        CancellationToken cancellationToken = default) =>
        await DbSet.Where(c => c.IsActive).ToListAsync(cancellationToken);
}
