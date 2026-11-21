// <copyright file="MfgRouting.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A production routing (revision) mirrored from the web-api (Mod 061), with its operation steps.
/// </summary>
public class MfgRouting : ITenantScopedEntity
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
    /// Gets or sets the routing number.
    /// </summary>
    public string RoutingNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the manufactured item identifier.
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Gets or sets the manufactured item code.
    /// </summary>
    public string? ItemCode { get; set; }

    /// <summary>
    /// Gets or sets the manufactured item name.
    /// </summary>
    public string? ItemName { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the revision.
    /// </summary>
    public string Revision { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the routing is the default one of the item.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets or sets the first effective day.
    /// </summary>
    public DateTime? EffectiveFrom { get; set; }

    /// <summary>
    /// Gets or sets the last effective day.
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// Gets or sets the notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the bill of materials identifier.
    /// </summary>
    public Guid? BomId { get; set; }

    /// <summary>
    /// Gets or sets when the row was pulled from the server.
    /// </summary>
    public DateTime SyncedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the operation steps.
    /// </summary>
    public ICollection<MfgRoutingOperation> Operations { get; set; } = [];
}
