// <copyright file="SyncEntityState.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Per-record sync metadata: delivery status and the server version the local copy is based on.
/// </summary>
public class SyncEntityState : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the state identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the sync entity name.
    /// </summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the described record.
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Gets or sets the delivery state of the record.
    /// </summary>
    public SyncOutboxStatus SyncStatus { get; set; } = SyncOutboxStatus.Pending;

    /// <summary>
    /// Gets or sets the server updated_at value the local copy is based on (optimistic concurrency token).
    /// </summary>
    public string? ServerVersion { get; set; }

    /// <summary>
    /// Gets or sets the last successful sync instant in UTC.
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
