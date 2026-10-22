// <copyright file="SwissQrBillRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository managing persistence and queries for Swiss QR-bill payment slips and settlement states.
/// </summary>
/// <param name="dbContext">The application database context.</param>
public sealed class SwissQrBillRepository(FatturaFacileDbContext dbContext)
    : Repository<SwissQrBill, Guid>(dbContext: dbContext), ISwissQrBillRepository
{

    /// <summary>
    /// Retrieves a Swiss QR-bill by its 27-digit SCOR reference number.
    /// </summary>
    /// <param name="referenceNumber">The 27-digit structured SCOR reference string.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The matching <see cref="SwissQrBill"/> if found; otherwise, <see langword="null"/>.</returns>
    public Task<SwissQrBill?> GetByReferenceNumberAsync(
        string            referenceNumber,
        CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(
                                  predicate: b => b.ReferenceNumber == referenceNumber,
                                  cancellationToken: cancellationToken);

    /// <summary>
    /// Retrieves all unpaid Swiss QR-bills within the current tenant scope.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only collection of unpaid Swiss QR-bill entities.</returns>
    public async Task<IReadOnlyList<SwissQrBill>> GetUnpaidBillsAsync(CancellationToken cancellationToken = default) =>
        await DbSet.Where(predicate: b => !b.IsPaid).ToListAsync(cancellationToken: cancellationToken);

}
