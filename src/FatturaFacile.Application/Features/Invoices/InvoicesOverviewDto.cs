// <copyright file="InvoicesOverviewDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Data transfer representation combining multi-jurisdiction sales billing documents for cross-border invoicing overviews.
/// </summary>
/// <param name="ItalianInvoices">The collection of Italian electronic invoices (FatturaPA).</param>
/// <param name="SwissQrBills">The collection of Swiss QR-bills.</param>
public record InvoicesOverviewDto(
    IReadOnlyList<ItalianInvoiceDto> ItalianInvoices,
    IReadOnlyList<SwissQrBillDto> SwissQrBills);
