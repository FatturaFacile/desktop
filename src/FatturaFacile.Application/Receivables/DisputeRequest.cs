// <copyright file="DisputeRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// Marks or clears the dispute of an item.
/// </summary>
/// <param name="CommandId">The identifier of the command, which is also its idempotency key.</param>
/// <param name="CustomerId">The customer.</param>
/// <param name="Reference">A label for the queue list.</param>
/// <param name="ItemId">The item.</param>
/// <param name="Disputed">Whether the item is disputed.</param>
/// <param name="DisputeReason">The reason, required when disputing.</param>
/// <param name="OccurredAtUtc">The instant the user acted.</param>
public sealed record DisputeRequest(
    Guid CommandId,
    Guid CustomerId,
    string Reference,
    Guid ItemId,
    bool Disputed,
    string? DisputeReason,
    DateTime OccurredAtUtc);
