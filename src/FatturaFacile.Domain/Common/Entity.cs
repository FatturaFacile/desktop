// <copyright file="Entity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Provides the foundational base class for domain entities defined by their identity rather than structural attributes.
/// </summary>
/// <typeparam name="TId">The type of the entity's unique identifier.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class for ORM materialization and serialization.
    /// </summary>
    protected Entity()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class with the specified unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier that defines the identity of this entity.</param>
    protected Entity(
        TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Gets or sets the unique identity key distinguishing this domain entity.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// Compares two entity instances for reference or identity equality.
    /// </summary>
    /// <param name="left">The first entity operand to compare.</param>
    /// <param name="right">The second entity operand to compare.</param>
    /// <returns><see langword="true"/> if both entities share reference or identical type and identity key; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(
        Entity<TId>? left,
        Entity<TId>? right) => Equals(
        left,
        right);

    /// <summary>
    /// Compares two entity instances for inequality based on identity.
    /// </summary>
    /// <param name="left">The first entity operand to compare.</param>
    /// <param name="right">The second entity operand to compare.</param>
    /// <returns><see langword="true"/> if the entities do not share identity; otherwise, <see langword="false"/>.</returns>
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
    /// Indicates whether the current entity is identical to another entity based on matching runtime type and identifier.
    /// </summary>
    /// <param name="other">An entity instance to compare with this entity.</param>
    /// <returns><see langword="true"/> if <paramref name="other"/> shares the same runtime type and <see cref="Id"/>; otherwise, <see langword="false"/>.</returns>
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
