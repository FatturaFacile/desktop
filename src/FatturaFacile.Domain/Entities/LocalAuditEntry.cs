// <copyright file="LocalAuditEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// An offline security audit record of the hash-chained local audit queue ("local_audit_queue").
/// The signed fields (sequence, hashes, payload, occurrence, entity, action, device) are immutable once written;
/// only the delivery fields change when the entry is synchronized.
/// </summary>
public class LocalAuditEntry : ITenantScopedEntity
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
    /// Gets or sets the position in the chain of the tenant (starts at 1, no gaps).
    /// </summary>
    public long SequenceNumber { get; set; }

    /// <summary>
    /// Gets or sets the record hash of the previous entry (64 zeros for the first one).
    /// </summary>
    public string PreviousHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SHA-256 (hex) of the canonical payload JSON.
    /// </summary>
    public string PayloadHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HMAC-SHA256 (hex) over previous hash, payload hash and metadata, keyed with the device key.
    /// </summary>
    public string RecordHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the canonical JSON payload the payload hash was computed from.
    /// </summary>
    public string PayloadJson { get; set; } = "null";

    /// <summary>
    /// Gets or sets the instant (UTC) the event happened on the device.
    /// </summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// Gets or sets the type of the entity the event is about (for example "contacts").
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the entity the event is about.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the action (for example "pii_unmask").
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the device that produced the entry.
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the delivery state.
    /// </summary>
    public AuditQueueStatus Status { get; set; } = AuditQueueStatus.Pending;

    /// <summary>
    /// Gets or sets the instant (UTC) the entry was acknowledged by the web-api.
    /// </summary>
    public DateTime? SyncedAt { get; set; }

    /// <summary>
    /// Gets or sets the server timestamp (UTC) at which the web-api recorded the entry. It is informative and never signed.
    /// </summary>
    public DateTime? ServerReceivedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier the web-api assigned to the entry, if any.
    /// </summary>
    public string? ServerReference { get; set; }

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
