// <copyright file="OrderReminderRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// Request to remind a supplier of an overdue order.
/// </summary>
/// <param name="CommandId">The idempotency key.</param>
/// <param name="OrderId">The order.</param>
/// <param name="Reference">The label shown in the queue.</param>
/// <param name="OccurredAtUtc">When the user acted (UTC).</param>
public sealed record OrderReminderRequest(
    Guid CommandId,
    Guid OrderId,
    string Reference,
    DateTime OccurredAtUtc);
