// <copyright file="AggregateRoot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Base class for Aggregate Roots managing domain events.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> domainEvents = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class.
    /// </summary>
    protected AggregateRoot()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The aggregate root identifier.</param>
    protected AggregateRoot(
        TId id)
        : base(id)
    {
    }

    /// <summary>
    /// Gets the read-only collection of domain events raised by this aggregate root.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the aggregate root's internal event queue.
    /// </summary>
    /// <param name="domainEvent">The domain event to append.</param>
    public void AddDomainEvent(
        IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all queued domain events from the aggregate root.
    /// </summary>
    public void ClearDomainEvents() => domainEvents.Clear();
}
