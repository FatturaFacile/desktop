// <copyright file="ICountryInvoicingStrategy.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Strategies;

/// <summary>
/// Strategy contract for jurisdiction-specific electronic and structured invoicing compliance.
/// </summary>
public interface ICountryInvoicingStrategy
{
    /// <summary>
    /// Gets the ISO country code this strategy handles ("CH" or "IT").
    /// </summary>
    public string CountryCode { get; }

    /// <summary>
    /// Enriches and processes compliance artifacts for the invoice (e.g. Swiss QR-Bill or Italian SDI FatturaPA XML).
    /// </summary>
    /// <param name="invoice">The invoice to process.</param>
    /// <param name="tenant">The issuing company tenant.</param>
    /// <param name="contact">The customer or partner contact.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous compliance processing.</returns>
    public Task ProcessInvoiceComplianceAsync(Invoice invoice, Tenant tenant, Contact contact, CancellationToken cancellationToken = default);
}
