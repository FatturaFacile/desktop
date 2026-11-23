// <copyright file="CreateDropshipmentRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// Request to create the supplier order of a dropship sales order.
/// </summary>
/// <param name="CommandId">The idempotency key.</param>
/// <param name="Reference">The label shown in the queue.</param>
/// <param name="SalesOrderId">The sales order.</param>
/// <param name="SupplierId">The supplier.</param>
/// <param name="ExpectedDeliveryDate">The expected delivery date.</param>
/// <param name="Currency">The currency.</param>
/// <param name="Notes">The notes.</param>
/// <param name="Address">The delivery address.</param>
/// <param name="Lines">The lines, empty for all the dropship lines.</param>
/// <param name="OccurredAtUtc">When the user acted (UTC).</param>
public sealed record CreateDropshipmentRequest(
    Guid CommandId,
    string Reference,
    Guid SalesOrderId,
    Guid SupplierId,
    DateOnly? ExpectedDeliveryDate,
    string? Currency,
    string? Notes,
    DropshipAddress Address,
    IReadOnlyList<DropshipLineRequest> Lines,
    DateTime OccurredAtUtc);
