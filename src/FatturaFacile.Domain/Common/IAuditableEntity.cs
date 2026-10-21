// <copyright file="IAuditableEntity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Audit trail tracking for created/modified timestamps and actors.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Gets or sets the UTC timestamp when the entity was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or system that created the entity.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or system that last updated the entity.
    /// </summary>
    public string? UpdatedBy { get; set; }
}
