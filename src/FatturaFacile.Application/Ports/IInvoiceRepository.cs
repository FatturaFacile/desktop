// <copyright file="IInvoiceRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Ports;

/// <summary>
/// Defines data access operations specific to invoice documents, line items, and sequential numbering sequences.
/// </summary>
public interface IInvoiceRepository : IRepository<Invoice>
{
    /// <summary>
    /// Retrieves all invoices for the current tenant including associated customer details and line items.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to observe during query execution.</param>
    /// <returns>A read-only collection of invoice entities populated with customer and line relationships.</returns>
    Task<IReadOnlyList<Invoice>> GetInvoicesWithDetailsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific invoice by identifier, including customer contact info, line items, and applied VAT breakdowns.
    /// </summary>
    /// <param name="id">The unique identifier of the invoice.</param>
    /// <param name="cancellationToken">Cancellation token to observe during query execution.</param>
    /// <returns>The fully populated invoice entity if found; otherwise, null.</returns>
    Task<Invoice?> GetInvoiceWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates the next sequential invoice number and formatted display string for a given fiscal year.
    /// </summary>
    /// <param name="fiscalYear">The fiscal year to determine the sequence for.</param>
    /// <param name="cancellationToken">Cancellation token to observe during the operation.</param>
    /// <returns>A tuple containing the formatted invoice number string and the raw sequential integer.</returns>
    Task<(string Number, int SequentialNumber)> GetNextInvoiceNumberAsync(int fiscalYear, CancellationToken cancellationToken = default);
}
