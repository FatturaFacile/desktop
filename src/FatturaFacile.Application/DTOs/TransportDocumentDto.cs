namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Represents transport document dto.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="TenantId">The tenant id.</param>
/// <param name="DocumentNumber">The document number.</param>
/// <param name="DocumentDate">The document date.</param>
/// <param name="ClientId">The client id.</param>
/// <param name="ClientName">The client name.</param>
/// <param name="DestinationAddressFormatted">The destination address formatted.</param>
/// <param name="TransportationReason">The transportation reason.</param>
/// <param name="CarrierName">The carrier name.</param>
/// <param name="NumberOfPackages">The number of packages.</param>
/// <param name="TotalWeightKg">The total weight kg.</param>
/// <param name="CustomsCmrNotes">The customs cmr notes.</param>
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
