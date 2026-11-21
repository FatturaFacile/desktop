// <copyright file="MfgRoutingOperationBomLink.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A bill of materials component linked to a routing operation.
/// </summary>
public class MfgRoutingOperationBomLink : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the server identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the local creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the local update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the operation identifier.
    /// </summary>
    public Guid OperationId { get; set; }

    /// <summary>
    /// Gets or sets the bill of materials component identifier.
    /// </summary>
    public Guid BomComponentId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the component is backflushed when the step completes.
    /// </summary>
    public bool BackflushOnCompletion { get; set; }

    /// <summary>
    /// Gets or sets the notes.
    /// </summary>
    public string? Notes { get; set; }
}
