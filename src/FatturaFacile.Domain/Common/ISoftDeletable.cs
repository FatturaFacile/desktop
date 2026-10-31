// <copyright file="ISoftDeletable.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Contract for entities supporting logical soft deletion matching Laravel Eloquent SoftDeletes.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity has been logically marked as deleted.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the entity was marked as deleted, or null if active.
    /// </summary>
    DateTime? DeletedAt { get; set; }
}
