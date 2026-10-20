namespace FatturaFacile.Application.DTOs;

public record TransportDocumentDto(
    Guid Id,
    Guid TenantId,
    string DocumentNumber,
    DateTime DocumentDate,
    Guid ClientId,
    string ClientName,
    string DestinationAddressFormatted,
    string TransportationReason,
    string CarrierName,
    int NumberOfPackages,
    decimal TotalWeightKg,
    string? CustomsCmrNotes);