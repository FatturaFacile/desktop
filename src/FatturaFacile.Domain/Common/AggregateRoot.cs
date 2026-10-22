// <copyright file="AggregateRoot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Represents the foundational base class for domain aggregate roots that encapsulate business invariants and manage domain events.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root's unique identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> domainEvents = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class for deserialization or ORM tracking.
    /// </summary>
    protected AggregateRoot()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class with the specified aggregate identifier.
    /// </summary>
    /// <param name="id">The unique identifier that distinguishes this aggregate root within the domain.</param>
    protected AggregateRoot(
        TId id)
        : base(id)
    {
    }

    /// <summary>
    /// Gets the read-only collection of uncommitted domain events raised during operations on this aggregate root.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => domainEvents.AsReadOnly();

    /// <summary>
    /// Records a domain event reflecting a state change within this aggregate root to be dispatched upon persistence.
    /// </summary>
    /// <param name="domainEvent">The domain event instance capturing the domain state transition.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="domainEvent"/> is <see langword="null"/>.</exception>
    public void AddDomainEvent(
        IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all uncommitted domain events from the aggregate root, typically invoked after successful dispatch or persistence.
    /// </summary>
    public void ClearDomainEvents() => domainEvents.Clear();
}
