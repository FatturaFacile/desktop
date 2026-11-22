// <copyright file="SendStatementRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// Sends the statement of a customer by email.
/// </summary>
/// <param name="CommandId">The identifier of the command, which is also its idempotency key.</param>
/// <param name="CustomerId">The customer.</param>
/// <param name="Reference">A label for the queue list.</param>
/// <param name="From">The first day of the period.</param>
/// <param name="To">The last day of the period.</param>
/// <param name="Recipient">The recipient, the customer email when empty.</param>
/// <param name="Locale">The language of the email: it, de, fr or en.</param>
/// <param name="OccurredAtUtc">The instant the user acted.</param>
public sealed record SendStatementRequest(
    Guid CommandId,
    Guid CustomerId,
    string Reference,
    DateOnly From,
    DateOnly To,
    string? Recipient,
    string Locale,
    DateTime OccurredAtUtc);
