using FatturaFacile.Domain.Shared.Repositories;
using FatturaFacile.Invoicing.Domain.Aggregates;

namespace FatturaFacile.Invoicing.Domain.Repositories;

public interface IInvoiceRepository : IRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByInvoiceNumberAsync(Guid tenantId, string invoiceNumber, CancellationToken cancellationToken = default);
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task DeleteAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid tenantId, string invoiceNumber, CancellationToken cancellationToken = default);
}
