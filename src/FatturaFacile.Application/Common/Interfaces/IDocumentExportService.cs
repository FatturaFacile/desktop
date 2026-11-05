// <copyright file="IDocumentExportService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Service abstraction for generating non-visual PDF documents and Excel workbooks.
/// </summary>
public interface IDocumentExportService
{
    /// <summary>
    /// Generates a PDF document for the specified invoice including QR-bill or SDI layout.
    /// </summary>
    /// <param name="invoice">The invoice details DTO.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The raw PDF byte array.</returns>
    public Task<byte[]> GenerateInvoicePdfAsync(InvoiceDto invoice, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports a tabular dataset of invoices to an Excel workbook (.xlsx).
    /// </summary>
    /// <param name="invoices">The invoices to include.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The raw Excel workbook byte array.</returns>
    public Task<byte[]> ExportInvoicesToExcelAsync(IEnumerable<InvoiceDto> invoices, CancellationToken cancellationToken = default);
}
