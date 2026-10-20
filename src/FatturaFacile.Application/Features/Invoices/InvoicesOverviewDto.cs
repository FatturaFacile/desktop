using FatturaFacile.Application.DTOs;

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Represents invoices overview dto.
/// </summary>
/// <param name="ItalianInvoices">The italian invoices.</param>
/// <param name="SwissQrBills">The swiss qr bills.</param>
public record InvoicesOverviewDto(
    IReadOnlyList<ItalianInvoiceDto> ItalianInvoices,
    IReadOnlyList<SwissQrBillDto> SwissQrBills);
