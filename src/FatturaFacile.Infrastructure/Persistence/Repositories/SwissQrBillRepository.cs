using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

public sealed class SwissQrBillRepository(FatturaFacileDbContext dbContext) : Repository<SwissQrBill, Guid>(dbContext), ISwissQrBillRepository
{
    public Task<SwissQrBill?> GetByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken = default) => DbSet.FirstOrDefaultAsync(b => b.ReferenceNumber == referenceNumber, cancellationToken);

    public async Task<IReadOnlyList<SwissQrBill>> GetUnpaidBillsAsync(CancellationToken cancellationToken = default) => await DbSet.Where(b => !b.IsPaid).ToListAsync(cancellationToken);
}