// <copyright file="ISoftDeletable.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Shared.Primitives;

/// <summary>
/// Represents entities supporting soft deletion.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Gets a value indicating whether this entity has been marked as deleted.
    /// </summary>
    public bool IsDeleted { get; }

    /// <summary>
    /// Gets the timestamp when this entity was marked as deleted.
    /// </summary>
    public DateTimeOffset? DeletedAt { get; }
}
