using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

public record ItalianInvoiceDto(
    Guid Id,
    Guid TenantId,
    string DocumentNumber,
    DateTime DocumentDate,
    string FormatoTrasmissione,
    string ProgressivoInvio,
    Guid ClientId,
    string CedenteDenominazione,
    string CedentePartitaIva,
    string CessionarioDenominazione,
    string? CessionarioPartitaIva,
    string CessionarioCodiceDestinatario,
    decimal TotalNet,
    decimal TotalVat,
    decimal TotalGross,
    string Currency,
    DateTime PaymentDueDate,
    SdiTransmissionStatus SdiStatus,
    string? SdiIdentifier,
    List<InvoiceLineDto> Lines);