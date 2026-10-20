using FatturaFacile.Domain.Entities.Sales;

namespace FatturaFacile.Domain.Repositories;

/// <summary>
/// Defines the contract for italian invoice repository.
/// </summary>
public interface IItalianInvoiceRepository : IRepository<ItalianElectronicInvoice, Guid>
{
    /// <summary>
    /// Get By Document Number Async.
    /// </summary>
    /// <param name="documentNumber">The document number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<ItalianElectronicInvoice?> GetByDocumentNumberAsync(string documentNumber, CancellationToken cancellationToken = default);

/// <summary>
    /// Get Pending Sdi Async.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<IReadOnlyList<ItalianElectronicInvoice>> GetPendingSdiAsync(CancellationToken cancellationToken = default);
}
