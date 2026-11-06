// <copyright file="TimeAuditEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// An immutable audit record of a change to the working time records (who, when, what, why).
/// </summary>
public class TimeAuditEntry : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the company (tenant) identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the employee identifier.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Gets or sets the changed record type.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the changed record identifier.
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Gets or sets the action: Created, Modified or Deleted.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the values before the change.
    /// </summary>
    public string? Before { get; set; }

    /// <summary>
    /// Gets or sets the values after the change.
    /// </summary>
    public string? After { get; set; }

    /// <summary>
    /// Gets or sets the reason given for the change.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Gets or sets the user who made the change.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the time of the change.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
