// <copyright file="ItalianInvoiceRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

/// <summary>
/// Represents italian invoice repository.
/// </summary>
/// <param name="dbContext">The db context.</param>
public sealed class ItalianInvoiceRepository(FatturaFacileDbContext dbContext)
    : Repository<ItalianElectronicInvoice, Guid>(dbContext), IItalianInvoiceRepository
{
    /// <summary>
    /// Get By Document Number Async.
    /// </summary>
    /// <param name="documentNumber">The document number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<ItalianElectronicInvoice?> GetByDocumentNumberAsync(
        string documentNumber,
        CancellationToken cancellationToken = default)
    {
        return DbSet.Include(i => i.Lines)
            .FirstOrDefaultAsync(
                i => i.DocumentNumber == documentNumber,
                cancellationToken);
    }

    /// <summary>
    /// Get Pending Sdi Async.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public async Task<IReadOnlyList<ItalianElectronicInvoice>> GetPendingSdiAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Include(i => i.Lines)
            .Where(i => i.SdiStatus == Domain.Enums.SdiTransmissionStatus.Draft ||
                        i.SdiStatus == Domain.Enums.SdiTransmissionStatus.ReadyToSend)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get All Async.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public override async Task<IReadOnlyList<ItalianElectronicInvoice>>
        GetAllAsync(
            CancellationToken cancellationToken = default) =>
        await DbSet.Include(i => i.Lines).ToListAsync(cancellationToken);
}
