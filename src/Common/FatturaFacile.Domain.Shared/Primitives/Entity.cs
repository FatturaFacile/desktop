using FatturaFacile.Domain.Shared.Abstractions;
using FatturaFacile.Domain.Shared.Events;

namespace FatturaFacile.Domain.Shared.Primitives;

/// <summary>
/// Legacy interface for backwards compatibility, inheriting from IMustHaveTenant.
/// </summary>
public interface IHasTenant : IMustHaveTenant
{
}

/// <summary>
/// Represents entities supporting soft deletion.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Gets a value indicating whether this entity has been marked as deleted.
    /// </summary>
    bool IsDeleted { get; }

    /// <summary>
    /// Gets the timestamp when this entity was marked as deleted.
    /// </summary>
    DateTimeOffset? DeletedAt { get; }
}

/// <summary>
/// Represents auditable entities tracking creation and mutation timestamps and actors.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Gets the creation timestamp in UTC.
    /// </summary>
    DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the identifier or username of the user who created the entity.
    /// </summary>
    string? CreatedBy { get; }

    /// <summary>
    /// Gets the last update timestamp in UTC.
    /// </summary>
    DateTimeOffset? UpdatedAt { get; }

    /// <summary>
    /// Gets the identifier or username of the user who last updated the entity.
    /// </summary>
    string? UpdatedBy { get; }
}

/// <summary>
/// Generic entity base class.
/// </summary>
/// <typeparam name="TId">The type of the primary key.</typeparam>
public abstract class Entity<TId> where TId : notnull
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode() => EqualityComparer<TId>.Default.GetHashCode(Id);

    public static bool operator ==(Entity<TId>? a, Entity<TId>? b) => a?.Equals(b) ?? b is null;
    public static bool operator !=(Entity<TId>? a, Entity<TId>? b) => !(a == b);
}

/// <summary>
/// Represents an aggregate root holding domain events and optimistic concurrency tokens.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root primary key.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets the collection of raised domain events pending persistence or dispatch.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Gets or sets the concurrency token row version.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];

    /// <summary>
    /// Raises a new domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event instance.</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all pending domain events.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
