// <copyright file="Product.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents an article, item, or billable service in catalog/inventory.
/// </summary>
public class Product : IOptionalTenantEntity
{

    /// <summary>
    /// Gets or sets the primary key identifier for the product.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the optional owning tenant identifier. If null, the product is global.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the stock keeping unit (SKU) code.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the barcode identifier (EAN/UPC).
    /// </summary>
    public string? Barcode { get; set; }

    /// <summary>
    /// Gets or sets the product name or title.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the unit of measure abbreviation (e.g., PZ, EA, KG, ORE).
    /// </summary>
    public string UnitMeasure { get; set; } = "PZ";

    /// <summary>
    /// Gets or sets the acquisition or production cost price.
    /// </summary>
    public decimal CostPrice { get; set; }

    /// <summary>
    /// Gets or sets the default sales price before tax.
    /// </summary>
    public decimal SalePrice { get; set; }

    /// <summary>
    /// Gets or sets the default VAT rate percentage (e.g. 22 or 8.1).
    /// </summary>
    public decimal TaxRate { get; set; } = 22m;

    /// <summary>
    /// Gets or sets the Harmonized System customs commodity code.
    /// </summary>
    public string? HsCode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether warehouse inventory is tracked for this item.
    /// </summary>
    public bool TrackInventory { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the product is currently active for sales.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets minimum stock safety threshold.
    /// </summary>
    public decimal? MinStock { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the product was created.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the product was updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the soft delete timestamp.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
