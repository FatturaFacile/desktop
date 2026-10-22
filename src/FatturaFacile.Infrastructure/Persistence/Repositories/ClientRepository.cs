// <copyright file="ClientRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Crm;
using FatturaFacile.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation managing client entity persistence, lookup queries by tax identifier, and active customer filtering.
/// </summary>
/// <param name="dbContext">The application database context.</param>
public sealed class ClientRepository(FatturaFacileDbContext dbContext)
    : Repository<Client, Guid>(dbContext: dbContext), IClientRepository
{

    /// <summary>
    /// Searches for a client matching the given Italian fiscal code, VAT number, or Swiss business UID.
    /// </summary>
    /// <param name="code">The alphanumeric tax identifier or Swiss UID to match against.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The matching <see cref="Client"/> entity if found; otherwise, <see langword="null"/>.</returns>
    public Task<Client?> GetByVatOrFiscalCodeAsync(
        string            code,
        CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(
                                         predicate: c => (c.FiscalCode != null && c.FiscalCode.Value == code) ||
                                                         (c.VatNumber  != null && c.VatNumber.Value  == code) ||
                                                         c.SwissUid == code,
                                         cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves all active clients for the current tenant boundary.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only collection of active <see cref="Client"/> entities.</returns>
    public async Task<IReadOnlyList<Client>> GetActiveClientsAsync(CancellationToken cancellationToken = default) =>
        await DbSet.Where(predicate: c => c.IsActive).ToListAsync(cancellationToken: cancellationToken);

}
