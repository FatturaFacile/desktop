// <copyright file="StockItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents an inventory item tracked for physical stock balance and moving average valuation.
/// </summary>
public class StockItem : ITenantScopedEntity
{

    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the mandatory owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the unique inventory SKU.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary EAN-13 barcode.
    /// </summary>
    public string? BarcodeEan13 { get; set; }

    /// <summary>
    /// Gets or sets the item name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the item description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the unit of measure code (e.g. "PCE", "KG", "L").
    /// </summary>
    public string Uom { get; set; } = "PCE";

    /// <summary>
    /// Gets or sets the standard baseline cost.
    /// </summary>
    public decimal StandardCost { get; set; }

    /// <summary>
    /// Gets or sets the current weighted moving average cost.
    /// </summary>
    public decimal CurrentMovingAverageCost { get; set; }

    /// <summary>
    /// Gets or sets the unit weight in kilograms.
    /// </summary>
    public decimal? WeightKg { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this item is active for stock movements.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the soft delete timestamp.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
