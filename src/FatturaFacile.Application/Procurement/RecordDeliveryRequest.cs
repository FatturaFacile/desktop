// <copyright file="RecordDeliveryRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// Request to record the delivery notice of the supplier of a dropshipment.
/// </summary>
/// <param name="CommandId">The idempotency key.</param>
/// <param name="DropshipmentId">The dropshipment.</param>
/// <param name="Reference">The label shown in the queue.</param>
/// <param name="DeliveredAt">The delivery date.</param>
/// <param name="DeliveryProofReference">The proof of delivery reference.</param>
/// <param name="SupplierShipmentReference">The shipment reference of the supplier.</param>
/// <param name="Lines">The delivered lines, empty for the whole open quantity.</param>
/// <param name="OccurredAtUtc">When the user acted (UTC).</param>
public sealed record RecordDeliveryRequest(
    Guid CommandId,
    Guid DropshipmentId,
    string Reference,
    DateOnly? DeliveredAt,
    string? DeliveryProofReference,
    string? SupplierShipmentReference,
    IReadOnlyList<DeliveryLineRequest> Lines,
    DateTime OccurredAtUtc);
