// <copyright file="DomainEventBase.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Provides a base record implementation for domain events.
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the event.
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the UTC timestamp when the domain event occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}
