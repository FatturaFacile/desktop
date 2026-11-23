// <copyright file="DeleteCreditLimitRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// Deletes a credit limit.
/// </summary>
/// <param name="CommandId">The identifier of the command, which is also its idempotency key.</param>
/// <param name="CustomerId">The customer.</param>
/// <param name="Reference">A label for the queue list.</param>
/// <param name="LimitId">The limit.</param>
/// <param name="OccurredAtUtc">The instant the user acted.</param>
public sealed record DeleteCreditLimitRequest(
    Guid CommandId,
    Guid CustomerId,
    string Reference,
    Guid LimitId,
    DateTime OccurredAtUtc);
