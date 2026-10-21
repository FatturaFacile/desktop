// <copyright file="Entity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Base class for domain entities.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class.
    /// </summary>
    protected Entity()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    protected Entity(
        TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// Determines whether two specified entities are equal.
    /// </summary>
    /// <param name="left">The first entity to compare.</param>
    /// <param name="right">The second entity to compare.</param>
    /// <returns><see langword="true"/> if the entities are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(
        Entity<TId>? left,
        Entity<TId>? right) => Equals(
        left,
        right);

    /// <summary>
    /// Determines whether two specified entities are not equal.
    /// </summary>
    /// <param name="left">The first entity to compare.</param>
    /// <param name="right">The second entity to compare.</param>
    /// <returns><see langword="true"/> if the entities are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(
        Entity<TId>? left,
        Entity<TId>? right) => !Equals(
        left,
        right);

    /// <inheritdoc/>
    public override bool Equals(
        object? obj)
    {
        if (obj is not Entity<TId> other)
        {
            return false;
        }

        return ReferenceEquals(
                   this,
                   other) ||
               (GetType() == other.GetType() && EqualityComparer<TId>.Default.Equals(
                   Id,
                   other.Id));
    }

    /// <summary>
    /// Indicates whether the current entity is equal to another entity of the same type.
    /// </summary>
    /// <param name="other">An entity to compare with this entity.</param>
    /// <returns><see langword="true"/> if the current entity is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.</returns>
    public virtual bool Equals(
        Entity<TId>? other)
    {
        if (other is null)
        {
            return false;
        }

        return ReferenceEquals(
                   this,
                   other) ||
               (GetType() == other.GetType() && EqualityComparer<TId>.Default.Equals(
                   Id,
                   other.Id));
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(
        GetType(),
        Id);
}
