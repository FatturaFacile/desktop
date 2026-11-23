// <copyright file="ApplyAllocationsRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// Applies allocations to open items.
/// </summary>
/// <param name="CommandId">The identifier of the command, which is also its idempotency key.</param>
/// <param name="CustomerId">The customer.</param>
/// <param name="Reference">A label for the queue list.</param>
/// <param name="Lines">The allocations.</param>
/// <param name="OccurredAtUtc">The instant the user acted.</param>
public sealed record ApplyAllocationsRequest(
    Guid CommandId,
    Guid CustomerId,
    string Reference,
    IReadOnlyList<AllocationLine> Lines,
    DateTime OccurredAtUtc);
