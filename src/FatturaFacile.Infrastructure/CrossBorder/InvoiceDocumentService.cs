// <copyright file="InvoiceDocumentService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Infrastructure.CrossBorder.FatturaPa;
using FatturaFacile.Infrastructure.CrossBorder.SwissQr;

namespace FatturaFacile.Infrastructure.CrossBorder;

/// <summary>
/// Provides document formatting and preview generation services for cross-border invoices,
/// including Italian FatturaPA XML and Swiss QR-bill SVG graphics.
/// </summary>
public sealed class InvoiceDocumentService : IInvoiceDocumentService
{

    /// <inheritdoc />
    public string GenerateFatturaPaXml(ItalianElectronicInvoice invoice) => FatturaPaXmlGenerator.GenerateXml(invoice: invoice);

    /// <inheritdoc />
    public string GenerateSwissQrSvg(SwissQrBill bill) => SwissQrBillGenerator.GenerateSvgPreview(bill: bill);

}
