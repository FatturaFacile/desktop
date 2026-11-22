// <copyright file="HelpdeskOutboxEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A helpdesk action (time log or escalation) saved locally until the server accepts it. Replayed in order when connectivity
/// returns; the idempotency key lets the server recognise a repeat.
/// </summary>
public class HelpdeskOutboxEntry : ITenantScopedEntity
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
    public HelpdeskAction Action { get; set; }

    /// <summary>
    /// Gets or sets the server identifier of the ticket.
    /// </summary>
    public Guid TicketId { get; set; }

    /// <summary>
    /// Gets or sets the human readable ticket number, for the queue list.
    /// </summary>
    public string TicketNumber { get; set; } = string.Empty;

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
    public HelpdeskOutboxStatus Status { get; set; } = HelpdeskOutboxStatus.Pending;

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
