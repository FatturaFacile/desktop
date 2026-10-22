// <copyright file="ItalianInvoiceRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository managing persistence and queries for Italian electronic sales invoices, including lines and SDI dispatch states.
/// </summary>
/// <param name="dbContext">The application database context.</param>
public sealed class ItalianInvoiceRepository(FatturaFacileDbContext dbContext)
    : Repository<ItalianElectronicInvoice, Guid>(dbContext: dbContext), IItalianInvoiceRepository
{

    /// <summary>
    /// Retrieves an Italian electronic invoice by its business document number, eagerly loading its line items.
    /// </summary>
    /// <param name="documentNumber">The business invoice number (e.g., "FATT-2026/001").</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The matching <see cref="ItalianElectronicInvoice"/> with lines if found; otherwise, <see langword="null"/>.</returns>
    public Task<ItalianElectronicInvoice?> GetByDocumentNumberAsync(
        string            documentNumber,
        CancellationToken cancellationToken = default)
    {
        return DbSet.Include(navigationPropertyPath: i => i.Lines)
                    .FirstOrDefaultAsync(
                                         predicate: i => i.DocumentNumber == documentNumber,
                                         cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves all invoices in <see cref="Domain.Enums.SdiTransmissionStatus.Draft"/> or <see cref="Domain.Enums.SdiTransmissionStatus.ReadyToSend"/> status.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only collection of pending electronic invoices with their lines.</returns>
    public async Task<IReadOnlyList<ItalianElectronicInvoice>> GetPendingSdiAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Include(navigationPropertyPath: i => i.Lines)
                          .Where(predicate: i => i.SdiStatus == Domain.Enums.SdiTransmissionStatus.Draft ||
                                                 i.SdiStatus == Domain.Enums.SdiTransmissionStatus.ReadyToSend)
                          .ToListAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves all Italian electronic invoices for the current tenant boundary, eagerly including invoice line items.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only collection of all electronic invoices with lines.</returns>
    public override async Task<IReadOnlyList<ItalianElectronicInvoice>>
        GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet.Include(navigationPropertyPath: i => i.Lines).ToListAsync(cancellationToken: cancellationToken);

}
