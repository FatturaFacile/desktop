using FatturaFacile.Domain.Entities.Sales;

namespace FatturaFacile.Domain.Repositories;

public interface ISwissQrBillRepository : IRepository<SwissQrBill, Guid>
{
    public Task<SwissQrBill?> GetByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<SwissQrBill>> GetUnpaidBillsAsync(CancellationToken cancellationToken = default);
}