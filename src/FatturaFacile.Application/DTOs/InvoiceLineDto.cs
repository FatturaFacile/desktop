namespace FatturaFacile.Application.DTOs;

public record InvoiceLineDto(
    int LineNumber,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal VatRate,
    string? NaturaExemptionCode,
    decimal LineTotal);