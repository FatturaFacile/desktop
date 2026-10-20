using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

/// <summary>
/// Represents swiss qr bill repository.
/// </summary>
/// <param name="dbContext">The db context.</param>
public sealed class SwissQrBillRepository(FatturaFacileDbContext dbContext) : Repository<SwissQrBill, Guid>(dbContext), ISwissQrBillRepository
{
/// <summary>
/// Get By Reference Number Async.
/// </summary>
/// <param name="referenceNumber">The reference number.</param>
/// <param name="cancellationToken">The cancellation token.</param>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<SwissQrBill?> GetByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken = default) => DbSet.FirstOrDefaultAsync(b => b.ReferenceNumber == referenceNumber, cancellationToken);

/// <summary>
/// Get Unpaid Bills Async.
/// </summary>
/// <param name="cancellationToken">The cancellation token.</param>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public async Task<IReadOnlyList<SwissQrBill>> GetUnpaidBillsAsync(CancellationToken cancellationToken = default) => await DbSet.Where(b => !b.IsPaid).ToListAsync(cancellationToken);
}
