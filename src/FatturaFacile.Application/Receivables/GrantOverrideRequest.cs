// <copyright file="GrantOverrideRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Receivables;

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// Grants a credit override.
/// </summary>
/// <param name="CommandId">The identifier of the command, which is also its idempotency key.</param>
/// <param name="CustomerId">The customer.</param>
/// <param name="Reference">A label for the queue list.</param>
/// <param name="Scope">The operations it covers.</param>
/// <param name="Reason">The reason, at least five characters.</param>
/// <param name="ExpiresAt">When it expires, when it does.</param>
/// <param name="OccurredAtUtc">The instant the user acted.</param>
public sealed record GrantOverrideRequest(
    Guid CommandId,
    Guid CustomerId,
    string Reference,
    CreditScope Scope,
    string Reason,
    DateTime? ExpiresAt,
    DateTime OccurredAtUtc);
