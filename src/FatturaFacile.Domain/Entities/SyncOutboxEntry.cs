// <copyright file="SyncOutboxEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A local change waiting to be replayed against the web-api (transactional outbox).
/// </summary>
public class SyncOutboxEntry : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the entry identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the sync entity name (for example "contacts").
    /// </summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the changed record.
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Gets or sets the queued operation.
    /// </summary>
    public SyncOperation Operation { get; set; }

    /// <summary>
    /// Gets or sets the delivery state.
    /// </summary>
    public SyncOutboxStatus Status { get; set; } = SyncOutboxStatus.Pending;

    /// <summary>
    /// Gets or sets the key sent as Idempotency-Key so that retries are never applied twice.
    /// </summary>
    public Guid IdempotencyKey { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the number of delivery attempts made.
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Gets or sets the earliest instant (UTC) of the next attempt.
    /// </summary>
    public DateTime NextAttemptAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last delivery error.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
