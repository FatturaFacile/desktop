// <copyright file="IBaseEntity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Defines the core contract for persisted domain entities with an identifier and audit timestamps.
/// </summary>
public interface IBaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier of the entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was created in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
