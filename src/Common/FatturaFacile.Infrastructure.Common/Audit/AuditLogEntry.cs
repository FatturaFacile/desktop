// <copyright file="AuditLogEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Infrastructure.Common.Audit;

/// <summary>
/// Represents an immutable persistent audit log record capturing state changes on domain entities.
/// </summary>
public sealed class AuditLogEntry
{
    /// <summary>
    /// Gets or sets the primary key identifier of the audit log entry.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the tenant identifier associated with the audited entity mutation, if applicable.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the authenticated user who initiated the action.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the name of the audited entity type.
    /// </summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the persistence action performed (e.g. Added, Modified, Deleted).
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary key value of the audited entity.
    /// </summary>
    public string PrimaryKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JSON-serialized representation of entity property values prior to modification.
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// Gets or sets the JSON-serialized representation of entity property values after modification.
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the audit record was captured.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
