// <copyright file="ISwissQrBillRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Sales;

namespace FatturaFacile.Domain.Repositories;

/// <summary>
/// Exposes specialized persistence and query operations for <see cref="SwissQrBill"/> aggregates complying with Swiss QR-bill (SIX Swico) payment standards.
/// </summary>
public interface ISwissQrBillRepository : IRepository<SwissQrBill, Guid>
{
    /// <summary>
    /// Asynchronously retrieves a Swiss QR-bill matching the specified structured or creditor payment reference.
    /// </summary>
    /// <param name="referenceNumber">The structured QR reference (27 digits with mod 10 recursive checksum) or Creditor Reference (ISO 11649 SCOR).</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous query.</param>
    /// <returns>A task representing the asynchronous lookup; contains the matching <see cref="SwissQrBill"/> if found, or <see langword="null"/> otherwise.</returns>
    public Task<SwissQrBill?> GetByReferenceNumberAsync(
        string referenceNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves all issued Swiss QR-bills that remain open, un-reconciled, or awaiting payment receipt.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous query.</param>
    /// <returns>A task representing the asynchronous query; contains a read-only list of outstanding, unpaid Swiss QR-bills.</returns>
    public Task<IReadOnlyList<SwissQrBill>> GetUnpaidBillsAsync(
        CancellationToken cancellationToken = default);
}
