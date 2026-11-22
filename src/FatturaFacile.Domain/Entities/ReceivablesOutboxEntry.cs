// <copyright file="ReceivablesOutboxEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A receivables write (allocation, reversal, dispute, credit limit, override, provision rates, statement email) saved locally
/// until the server accepts it. Replayed in order when connectivity returns; the idempotency key is unique per tenant, so the same
/// command can never be queued, and therefore never replayed, twice.
/// </summary>
public class ReceivablesOutboxEntry : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the entry identifier, which is also the idempotency key.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <inheritdoc/>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the action.
    /// </summary>
    public ReceivablesAction Action { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method (POST, PUT or DELETE).
    /// </summary>
    public string HttpMethod { get; set; } = "POST";

    /// <summary>
    /// Gets or sets the route relative to the API root.
    /// </summary>
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer the action concerns.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the human readable label for the queue list.
    /// </summary>
    public string Reference { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JSON request body (snake_case, without the idempotency key).
    /// </summary>
    public string PayloadJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the key that makes a resend safe.
    /// </summary>
    public Guid IdempotencyKey { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the instant the user acted.
    /// </summary>
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    public ReceivablesOutboxStatus Status { get; set; } = ReceivablesOutboxStatus.Pending;

    /// <summary>
    /// Gets or sets the number of failed attempts.
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Gets or sets the earliest instant of the next attempt.
    /// </summary>
    public DateTime NextAttemptAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last error.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Gets or sets the JSON answer of the server once the entry was accepted.
    /// </summary>
    public string? ResultJson { get; set; }

    /// <inheritdoc/>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <inheritdoc/>
    public DateTime? UpdatedAt { get; set; }
}
