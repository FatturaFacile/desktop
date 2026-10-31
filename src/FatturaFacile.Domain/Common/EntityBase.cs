// <copyright file="EntityBase.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Base class for all domain entities providing strongly typed GUID identity and timestamps.
/// </summary>
public abstract class EntityBase
{
    /// <summary>
    /// Gets or sets the unique primary key identifier for the entity.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the date and time when the entity was originally created in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the entity was last modified in UTC, or null if never modified.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
