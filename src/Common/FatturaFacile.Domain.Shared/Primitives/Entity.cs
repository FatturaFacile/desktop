// <copyright file="Entity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Shared.Primitives;

/// <summary>
/// Generic entity base class identified by a unique primary key.
/// </summary>
/// <typeparam name="TId">The type of the primary key.</typeparam>
public abstract class Entity<TId>
    where TId : notnull
{
    /// <summary>
    /// Gets or sets the primary key identifier of the entity.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// Determines whether the specified object is equal to the current entity based on identifier and type.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns><see langword="true"/> if entities have the same identity and concrete type; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is Entity<TId> other && (ReferenceEquals(this, other) || (GetType() == other.GetType() && EqualityComparer<TId>.Default.Equals(Id, other.Id)));

    /// <summary>
    /// Serves as the hash function for the entity using its identifier.
    /// </summary>
    /// <returns>A hash code based on the entity's primary key.</returns>
    public override int GetHashCode() => EqualityComparer<TId>.Default.GetHashCode(Id);
}
