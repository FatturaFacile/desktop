namespace FatturaFacile.Application.Features.Invoices;

public record CreateInvoiceLineItem(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal VatRate,
    string? Natura = null);