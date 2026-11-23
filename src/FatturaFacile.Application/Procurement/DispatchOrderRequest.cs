// <copyright file="DispatchOrderRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// Request to transmit an approved purchase order to its supplier.
/// </summary>
/// <param name="CommandId">The idempotency key.</param>
/// <param name="OrderId">The order.</param>
/// <param name="Reference">The label shown in the queue (the order number).</param>
/// <param name="Channel">The channel: pdf, email, edi or peppol.</param>
/// <param name="Recipient">The recipient, required for email.</param>
/// <param name="OccurredAtUtc">When the user acted (UTC).</param>
public sealed record DispatchOrderRequest(
    Guid CommandId,
    Guid OrderId,
    string Reference,
    string Channel,
    string? Recipient,
    DateTime OccurredAtUtc);
