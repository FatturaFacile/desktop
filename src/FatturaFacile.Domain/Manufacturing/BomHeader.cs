// <copyright file="BomHeader.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Manufacturing;

/// <summary>
/// A bill of materials header pulled from the web-api (multi-level through component items that own a BOM).
/// </summary>
public class BomHeader : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the local identifier (equals the server identifier for pulled records).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the local tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the BOM number.
    /// </summary>
    public string BomNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the produced item identifier.
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Gets or sets the produced item code.
    /// </summary>
    public string ItemCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the produced item name.
    /// </summary>
    public string ItemName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the revision label.
    /// </summary>
    public string Revision { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the server status (draft, active, obsolete).
    /// </summary>
    public string Status { get; set; } = "draft";

    /// <summary>
    /// Gets or sets the quantity the component quantities refer to.
    /// </summary>
    public decimal BaseQuantity { get; set; } = 1m;

    /// <summary>
    /// Gets or sets the unit of measure.
    /// </summary>
    public string Uom { get; set; } = "PZ";

    /// <summary>
    /// Gets or sets the components.
    /// </summary>
    public ICollection<BomLine> Lines { get; set; } = [];

    /// <summary>
    /// Gets or sets the creation instant (UTC).
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last local modification instant (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the server updated_at stamp seen at the last pull.
    /// </summary>
    public string? ServerVersion { get; set; }
}
