// <copyright file="FieldServiceOutboxEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A field operation waiting in the local outbox until the web-api applies it through `POST /api/v1/fsm/mobile-sync`. The
/// technician acted at <see cref="OccurredAtUtc"/>; <see cref="ClientUuid"/> is sent as the `client_uuid` of the operation, so a
/// replay after a lost answer is recognised by the server as a duplicate and applied only once.
/// </summary>
public class FieldServiceOutboxEntry : ITenantScopedEntity
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
    /// Gets or sets the kind of operation.
    /// </summary>
    public FieldServiceOperationType Operation { get; set; }

    /// <summary>
    /// Gets or sets the work order the operation belongs to.
    /// </summary>
    public Guid WorkOrderId { get; set; }

    /// <summary>
    /// Gets or sets the `payload` object of the operation, as JSON.
    /// </summary>
    public string PayloadJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the client identifier sent as idempotency key (at most 64 characters).
    /// </summary>
    public string ClientUuid { get; set; } = Guid.NewGuid().ToString("N", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets or sets the instant the technician acted (UTC).
    /// </summary>
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    public FieldServiceOutboxStatus Status { get; set; } = FieldServiceOutboxStatus.Pending;

    /// <summary>
    /// Gets or sets the number of failed delivery attempts.
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Gets or sets the earliest instant of the next attempt.
    /// </summary>
    public DateTime NextAttemptAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last error reported by the server.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Gets or sets the creation instant.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last change instant.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
