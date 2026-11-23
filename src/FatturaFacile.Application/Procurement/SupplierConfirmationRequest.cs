// <copyright file="SupplierConfirmationRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// Request to record the order confirmation received from a supplier.
/// </summary>
/// <param name="CommandId">The idempotency key.</param>
/// <param name="OrderId">The order.</param>
/// <param name="Reference">The label shown in the queue.</param>
/// <param name="ConfirmationNumber">The number given by the supplier.</param>
/// <param name="ConfirmedAt">The date of the confirmation.</param>
/// <param name="ConfirmedDeliveryDate">The promised delivery date of the order.</param>
/// <param name="Notes">The notes.</param>
/// <param name="Lines">The confirmed lines, empty when the supplier confirms as ordered.</param>
/// <param name="OccurredAtUtc">When the user acted (UTC).</param>
public sealed record SupplierConfirmationRequest(
    Guid CommandId,
    Guid OrderId,
    string Reference,
    string ConfirmationNumber,
    DateOnly? ConfirmedAt,
    DateOnly? ConfirmedDeliveryDate,
    string? Notes,
    IReadOnlyList<ConfirmationLineRequest> Lines,
    DateTime OccurredAtUtc);
