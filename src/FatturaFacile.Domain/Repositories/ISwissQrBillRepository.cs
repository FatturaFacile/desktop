using FatturaFacile.Domain.Entities.Sales;

namespace FatturaFacile.Domain.Repositories;

/// <summary>
/// Defines the contract for swiss qr bill repository.
/// </summary>
public interface ISwissQrBillRepository : IRepository<SwissQrBill, Guid>
{
    /// <summary>
    /// Get By Reference Number Async.
    /// </summary>
    /// <param name="referenceNumber">The reference number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<SwissQrBill?> GetByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken = default);

/// <summary>
    /// Get Unpaid Bills Async.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<IReadOnlyList<SwissQrBill>> GetUnpaidBillsAsync(CancellationToken cancellationToken = default);
}
