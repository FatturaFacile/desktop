// <copyright file="IAuditableEntity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Shared.Primitives;

/// <summary>
/// Represents auditable entities tracking creation and mutation timestamps and actors.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Gets the creation timestamp in UTC.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the identifier or username of the user who created the entity.
    /// </summary>
    public string? CreatedBy { get; }

    /// <summary>
    /// Gets the last update timestamp in UTC.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; }

    /// <summary>
    /// Gets the identifier or username of the user who last updated the entity.
    /// </summary>
    public string? UpdatedBy { get; }
}
