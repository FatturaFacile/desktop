using Microsoft.EntityFrameworkCore;
using FatturaFacile.Invoicing.Domain.Aggregates;
using FatturaFacile.Invoicing.Domain.Repositories;
using FatturaFacile.Invoicing.Infrastructure.Persistence;

namespace FatturaFacile.Invoicing.Infrastructure.Repositories;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly InvoicingDbContext _context;

    public InvoiceRepository(InvoicingDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(Guid tenantId, string invoiceNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.InvoiceNumber == invoiceNumber, cancellationToken);
    }

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        await _context.Invoices.AddAsync(invoice, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid tenantId, string invoiceNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .AnyAsync(i => i.TenantId == tenantId && i.InvoiceNumber == invoiceNumber, cancellationToken);
    }
}
