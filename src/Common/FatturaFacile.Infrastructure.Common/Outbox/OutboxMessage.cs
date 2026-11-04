// <copyright file="OutboxMessage.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Infrastructure.Common.Outbox;

/// <summary>
/// Represents a serialized domain event persisted within the transactional outbox table for reliable processing.
/// </summary>
public sealed class OutboxMessage
{
    /// <summary>
    /// Gets or sets the primary key identifier of the outbox message.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the assembly qualified or short type name of the serialized domain event.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JSON serialized content of the domain event.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC timestamp when the domain event occurred.
    /// </summary>
    public DateTimeOffset OccurredOnUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the UTC timestamp when the outbox message was processed and published, or null if pending.
    /// </summary>
    public DateTimeOffset? ProcessedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the error details if message processing or dispatch failed.
    /// </summary>
    public string? Error { get; set; }
}
