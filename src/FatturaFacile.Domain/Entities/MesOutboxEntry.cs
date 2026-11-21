// <copyright file="MesOutboxEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A MES operator command waiting in the local outbox until the web-api accepts it. The operator acted at
/// <see cref="OccurredAtUtc"/>; the server computes times from that instant, so a command replayed after an offline period
/// still measures the real time. <see cref="IdempotencyKey"/> makes a retry after a lost answer harmless.
/// </summary>
public class MesOutboxEntry : ITenantScopedEntity
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
    /// Gets or sets the action.
    /// </summary>
    public MesAction Action { get; set; }

    /// <summary>
    /// Gets or sets the production order operation the phase belongs to.
    /// </summary>
    public Guid OperationId { get; set; }

    /// <summary>
    /// Gets or sets the server phase identifier, null until a take-over created the phase.
    /// </summary>
    public Guid? PhaseId { get; set; }

    /// <summary>
    /// Gets or sets the command fields, as JSON (the idempotency key and the instant are added when it is sent).
    /// </summary>
    public string PayloadJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the key sent as idempotency key so that retries are never applied twice.
    /// </summary>
    public Guid IdempotencyKey { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the instant the operator acted (UTC).
    /// </summary>
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    public MesOutboxStatus Status { get; set; } = MesOutboxStatus.Pending;

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
    /// Gets or sets the creation instant.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last change instant.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
