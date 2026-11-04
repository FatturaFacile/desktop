// <copyright file="IInvoiceRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Repositories;
using FatturaFacile.Invoicing.Domain.Aggregates;

namespace FatturaFacile.Invoicing.Domain.Repositories;

/// <summary>
/// Domain repository contract for managing <see cref="Invoice"/> aggregate roots.
/// </summary>
public interface IInvoiceRepository : IRepository
{
    /// <summary>
    /// Retrieves an invoice by its unique primary key identifier along with its line items.
    /// </summary>
    /// <param name="id">The invoice identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching <see cref="Invoice"/> if found; otherwise, null.</returns>
    public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an invoice for a specific tenant by its document reference number.
    /// </summary>
    /// <param name="tenantId">The owning tenant identifier.</param>
    /// <param name="invoiceNumber">The invoice reference number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching <see cref="Invoice"/> if found; otherwise, null.</returns>
    public Task<Invoice?> GetByInvoiceNumberAsync(Guid tenantId, string invoiceNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new invoice aggregate root to the repository.
    /// </summary>
    /// <param name="invoice">The invoice instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing invoice in the repository.
    /// </summary>
    /// <param name="invoice">The invoice instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an invoice from the repository.
    /// </summary>
    /// <param name="invoice">The invoice instance to remove.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task DeleteAsync(Invoice invoice, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether an invoice with the specified document number exists for a given tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="invoiceNumber">The invoice reference number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the invoice exists; otherwise, <see langword="false"/>.</returns>
    public Task<bool> ExistsAsync(Guid tenantId, string invoiceNumber, CancellationToken cancellationToken = default);
}
