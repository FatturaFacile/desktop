// <copyright file="ReverseAllocationRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// Reverses an allocation.
/// </summary>
/// <param name="CommandId">The identifier of the command, which is also its idempotency key.</param>
/// <param name="CustomerId">The customer.</param>
/// <param name="Reference">A label for the queue list.</param>
/// <param name="AllocationId">The allocation.</param>
/// <param name="OccurredAtUtc">The instant the user acted.</param>
public sealed record ReverseAllocationRequest(
    Guid CommandId,
    Guid CustomerId,
    string Reference,
    Guid AllocationId,
    DateTime OccurredAtUtc);
