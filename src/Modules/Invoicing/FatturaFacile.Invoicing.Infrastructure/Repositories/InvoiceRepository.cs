// <copyright file="InvoiceRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Invoicing.Domain.Aggregates;
using FatturaFacile.Invoicing.Domain.Repositories;
using FatturaFacile.Invoicing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Invoicing.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of the <see cref="IInvoiceRepository"/> interface.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InvoiceRepository"/> class.
/// </remarks>
/// <param name="context">The invoicing database context.</param>
public sealed class InvoiceRepository(InvoicingDbContext context) : IInvoiceRepository
{
    private readonly InvoicingDbContext _context = context;

    /// <inheritdoc />
    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Invoice?> GetByInvoiceNumberAsync(Guid tenantId, string invoiceNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.InvoiceNumber == invoiceNumber, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        _ = await _context.Invoices.AddAsync(invoice, cancellationToken);
        _ = await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        _ = _context.Invoices.Update(invoice);
        _ = await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        _ = _context.Invoices.Remove(invoice);
        _ = await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(Guid tenantId, string invoiceNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .AnyAsync(i => i.TenantId == tenantId && i.InvoiceNumber == invoiceNumber, cancellationToken);
    }
}
