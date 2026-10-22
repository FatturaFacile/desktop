// <copyright file="IItalianInvoiceRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Sales;

namespace FatturaFacile.Domain.Repositories;

/// <summary>
/// Exposes specialized persistence and query operations for <see cref="ItalianElectronicInvoice"/> aggregates complying with Italian SDI (Sistema di Interscambio) specifications.
/// </summary>
public interface IItalianInvoiceRepository : IRepository<ItalianElectronicInvoice, Guid>
{

    /// <summary>
    /// Asynchronously retrieves an Italian electronic invoice by its unique sequential document number within the current tenant and fiscal year.
    /// </summary>
    /// <param name="documentNumber">The fiscal invoice or credit note document identifier (Numero Documento, e.g., '2026/001').</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous query.</param>
    /// <returns>A task representing the asynchronous lookup; contains the matching <see cref="ItalianElectronicInvoice"/> if found, or <see langword="null"/> otherwise.</returns>
    public Task<ItalianElectronicInvoice?> GetByDocumentNumberAsync(
        string            documentNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves all electronic invoices awaiting transmission, acknowledgment, or delivery response from the Italian SDI exchange hub.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous query.</param>
    /// <returns>A task representing the asynchronous query; contains a read-only list of invoices pending SDI processing.</returns>
    public Task<IReadOnlyList<ItalianElectronicInvoice>> GetPendingSdiAsync(
        CancellationToken cancellationToken = default);

}
