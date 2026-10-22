// <copyright file="TransportDocumentDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of a transport document (DDT / Bolla di Accompagnamento / Delivery Note).
/// </summary>
/// <param name="Id">The unique transport document identifier.</param>
/// <param name="TenantId">The tenant organization boundary owning this shipping document.</param>
/// <param name="DocumentNumber">The sequential document number identifying the transport note.</param>
/// <param name="DocumentDate">The issuance and dispatch date of the goods shipment.</param>
/// <param name="ClientId">The unique identifier of the destination client.</param>
/// <param name="ClientName">The commercial or legal name of the recipient client.</param>
/// <param name="DestinationAddressFormatted">The full delivery destination address string.</param>
/// <param name="TransportationReason">The statutory causal reason for transportation (e.g., Sale, Transfer, Repair, Consignment).</param>
/// <param name="CarrierName">The name of the freight carrier, transport operator, or shipper.</param>
/// <param name="NumberOfPackages">The total number of parcels or physical packages in the shipment.</param>
/// <param name="TotalWeightKg">The total gross shipment weight in kilograms.</param>
/// <param name="CustomsCmrNotes">Optional CMR or cross-border customs declarations and consignment notes.</param>
public record TransportDocumentDto(
    Guid     Id,
    Guid     TenantId,
    string   DocumentNumber,
    DateTime DocumentDate,
    Guid     ClientId,
    string   ClientName,
    string   DestinationAddressFormatted,
    string   TransportationReason,
    string   CarrierName,
    int      NumberOfPackages,
    decimal  TotalWeightKg,
    string?  CustomsCmrNotes);
