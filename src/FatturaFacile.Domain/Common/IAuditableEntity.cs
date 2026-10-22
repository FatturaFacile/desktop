// <copyright file="IAuditableEntity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Defines audit tracking metadata for entities capturing creation and modification timestamps alongside actor identities.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Gets or sets the coordinated universal time (UTC) timestamp when the entity record was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the unique username or system identity that created the entity record.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the coordinated universal time (UTC) timestamp when the entity record was last modified, or <see langword="null"/> if unchanged since creation.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the unique username or system identity that last modified the entity record, or <see langword="null"/> if unedited.
    /// </summary>
    public string? UpdatedBy { get; set; }
}
