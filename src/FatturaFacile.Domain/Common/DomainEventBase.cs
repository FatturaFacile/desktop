// <copyright file="DomainEventBase.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Provides a base immutable record implementation for domain events capturing significant business occurrences.
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{

    /// <summary>
    /// Gets the globally unique identifier assigned to this domain event instance for idempotency and tracing.
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the coordinated universal time (UTC) timestamp when the business event was instantiated.
    /// </summary>
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;

}
