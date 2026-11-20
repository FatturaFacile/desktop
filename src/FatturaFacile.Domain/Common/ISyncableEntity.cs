// <copyright file="ISyncableEntity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Marks an entity that is mirrored with the web-api by the offline-first sync engine.
/// </summary>
public interface ISyncableEntity
{
    /// <summary>
    /// Gets or sets the unique identifier shared with the server.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the logical deletion instant in UTC.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
