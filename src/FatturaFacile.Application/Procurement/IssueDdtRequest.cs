// <copyright file="IssueDdtRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// Request to issue the custom transport document of a dropshipment.
/// </summary>
/// <param name="CommandId">The idempotency key.</param>
/// <param name="DropshipmentId">The dropshipment.</param>
/// <param name="Reference">The label shown in the queue.</param>
/// <param name="Carrier">The carrier.</param>
/// <param name="TrackingNumber">The tracking number.</param>
/// <param name="Packages">The number of packages.</param>
/// <param name="GrossWeight">The gross weight.</param>
/// <param name="TransportBy">Who transports the goods.</param>
/// <param name="ShipmentDate">The shipment date.</param>
/// <param name="OccurredAtUtc">When the user acted (UTC).</param>
public sealed record IssueDdtRequest(
    Guid CommandId,
    Guid DropshipmentId,
    string Reference,
    string? Carrier,
    string? TrackingNumber,
    int? Packages,
    decimal? GrossWeight,
    string? TransportBy,
    DateOnly? ShipmentDate,
    DateTime OccurredAtUtc);
