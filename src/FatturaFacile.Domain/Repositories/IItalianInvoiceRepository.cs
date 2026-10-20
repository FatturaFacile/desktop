using FatturaFacile.Domain.Entities.Sales;

namespace FatturaFacile.Domain.Repositories;

public interface IItalianInvoiceRepository : IRepository<ItalianElectronicInvoice, Guid>
{
    public Task<ItalianElectronicInvoice?> GetByDocumentNumberAsync(string documentNumber, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<ItalianElectronicInvoice>> GetPendingSdiAsync(CancellationToken cancellationToken = default);
}