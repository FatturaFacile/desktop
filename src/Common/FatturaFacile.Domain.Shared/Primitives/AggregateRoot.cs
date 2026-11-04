// <copyright file="AggregateRoot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Events;

namespace FatturaFacile.Domain.Shared.Primitives;

/// <summary>
/// Represents an aggregate root that supports domain events and optimistic concurrency.
/// </summary>
/// <typeparam name="TId">The aggregate identifier type.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents =  [];

    /// <summary>
    /// Gets the collection of domain events raised by the aggregate and pending dispatch.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Gets or sets the optimistic concurrency token.
    /// </summary>
    public IList<byte> RowVersion { get; set; } = [];

    /// <summary>
    /// Removes all pending domain events from the aggregate.
    /// Typically invoked after successful event dispatch.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Adds a domain event to the aggregate event collection.
    /// The event will be dispatched by the infrastructure layer after the current unit of work completes successfully.
    /// </summary>
    /// <param name="domainEvent">The domain event to register.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="domainEvent"/> is null.
    /// </exception>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _domainEvents.Add(domainEvent);
    }
}
