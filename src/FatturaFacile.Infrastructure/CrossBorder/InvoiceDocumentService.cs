// <copyright file="InvoiceDocumentService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Infrastructure.CrossBorder.FatturaPa;
using FatturaFacile.Infrastructure.CrossBorder.SwissQr;

namespace FatturaFacile.Infrastructure.CrossBorder;

/// <summary>
/// Implementation of invoice document and preview generation.
/// </summary>
public sealed class InvoiceDocumentService : IInvoiceDocumentService
{
    /// <inheritdoc />
    public string GenerateFatturaPaXml(ItalianElectronicInvoice invoice) => FatturaPaXmlGenerator.GenerateXml(invoice);

    /// <inheritdoc />
    public string GenerateSwissQrSvg(SwissQrBill bill) => SwissQrBillGenerator.GenerateSvgPreview(bill);
}
