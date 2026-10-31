// <copyright file="IInvoiceDocumentService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Sales;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Service contract for generating invoice documents and visual previews.
/// </summary>
public interface IInvoiceDocumentService
{

    /// <summary>
    /// Generates XML for an Italian electronic invoice (FatturaPA).
    /// </summary>
    /// <param name="invoice">The Italian electronic invoice entity.</param>
    /// <returns>The generated XML string.</returns>
    public string GenerateFatturaPaXml(ItalianElectronicInvoice invoice);

    /// <summary>
    /// Generates SVG preview for a Swiss QR-bill.
    /// </summary>
    /// <param name="bill">The Swiss QR-bill entity.</param>
    /// <returns>The generated SVG string.</returns>
    public string GenerateSwissQrSvg(SwissQrBill bill);

}
