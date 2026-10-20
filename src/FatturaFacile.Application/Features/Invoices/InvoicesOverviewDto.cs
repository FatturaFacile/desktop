using FatturaFacile.Application.DTOs;

namespace FatturaFacile.Application.Features.Invoices;

public record InvoicesOverviewDto(
    IReadOnlyList<ItalianInvoiceDto> ItalianInvoices,
    IReadOnlyList<SwissQrBillDto> SwissQrBills);