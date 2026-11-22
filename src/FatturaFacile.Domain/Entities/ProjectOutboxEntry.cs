// <copyright file="ProjectOutboxEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A project change made offline and waiting to be sent to the web-api (Mod 078); the idempotency key makes a replay safe.
/// </summary>
public class ProjectOutboxEntry : ITenantScopedEntity
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
    /// Gets or sets the queued action.
    /// </summary>
    public ProjectOutboxAction Action { get; set; }

    /// <summary>
    /// Gets or sets the project the change belongs to.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the node, dependency or baseline the change targets (the project itself for project changes).
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Gets or sets the project code, shown in the pending list.
    /// </summary>
    public string ProjectCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the request body.
    /// </summary>
    public string PayloadJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the idempotency key sent with the request.
    /// </summary>
    public Guid IdempotencyKey { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the instant (UTC) the change was made.
    /// </summary>
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the delivery status.
    /// </summary>
    public ProjectOutboxStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the number of delivery attempts.
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Gets or sets the instant (UTC) before which the entry is not retried.
    /// </summary>
    public DateTime NextAttemptAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last delivery error.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Gets or sets the answer of the web-api.
    /// </summary>
    public string? ResultJson { get; set; }

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
