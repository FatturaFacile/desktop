// <copyright file="RevokeOverrideRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// Revokes a credit override.
/// </summary>
/// <param name="CommandId">The identifier of the command, which is also its idempotency key.</param>
/// <param name="CustomerId">The customer.</param>
/// <param name="Reference">A label for the queue list.</param>
/// <param name="OverrideId">The override.</param>
/// <param name="RevokeReason">The reason.</param>
/// <param name="OccurredAtUtc">The instant the user acted.</param>
public sealed record RevokeOverrideRequest(
    Guid CommandId,
    Guid CustomerId,
    string Reference,
    Guid OverrideId,
    string? RevokeReason,
    DateTime OccurredAtUtc);
