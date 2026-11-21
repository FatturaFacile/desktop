// <copyright file="ManufacturingOutboxEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Manufacturing;

/// <summary>
/// A queued manufacturing mutation. The payload and the idempotency key are stored with the row and never regenerated.
/// </summary>
public class ManufacturingOutboxEntry : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the local identifier (equals the server identifier for pulled records).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the local tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the FIFO position within the outbox.
    /// </summary>
    public long Sequence { get; set; }

    /// <summary>
    /// Gets or sets the kind of mutation.
    /// </summary>
    public ManufacturingMutationKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the local order identifier.
    /// </summary>
    public Guid ProductionOrderId { get; set; }

    /// <summary>
    /// Gets or sets the operation identifier (confirmations only).
    /// </summary>
    public Guid? OperationId { get; set; }

    /// <summary>
    /// Gets or sets the relative route; {order} is replaced by the server order identifier at send time.
    /// </summary>
    public string RouteTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JSON body.
    /// </summary>
    public string? PayloadJson { get; set; }

    /// <summary>
    /// Gets or sets the idempotency key sent on every attempt.
    /// </summary>
    public Guid IdempotencyKey { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the delivery state.
    /// </summary>
    public ManufacturingOutboxStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the transient failures so far.
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Gets or sets the conflict re-applications so far.
    /// </summary>
    public int ConflictRetries { get; set; }

    /// <summary>
    /// Gets or sets the earliest next attempt (UTC).
    /// </summary>
    public DateTime NextAttemptAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last error.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Gets or sets how a conflict was resolved.
    /// </summary>
    public string? Resolution { get; set; }

    /// <summary>
    /// Gets or sets the server stamp of the order when the mutation was queued.
    /// </summary>
    public string? BaseServerVersion { get; set; }

    /// <summary>
    /// Gets or sets the creation instant (UTC).
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last local modification instant (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
