// <copyright file="ProductDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer object representing a catalog product or service.
/// </summary>
public sealed class ProductDto
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the optional owning tenant identifier.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the stock keeping unit (SKU).
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the barcode.
    /// </summary>
    public string? Barcode { get; set; }

    /// <summary>
    /// Gets or sets the product name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the unit of measure (e.g. PZ, EA, KG, ORE).
    /// </summary>
    public string UnitMeasure { get; set; } = "PZ";

    /// <summary>
    /// Gets or sets the cost price.
    /// </summary>
    public decimal CostPrice { get; set; }

    /// <summary>
    /// Gets or sets the selling price before tax.
    /// </summary>
    public decimal SalePrice { get; set; }

    /// <summary>
    /// Gets or sets the standard tax rate percentage.
    /// </summary>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Gets or sets the Harmonized System customs code.
    /// </summary>
    public string? HsCode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether stock inventory is tracked.
    /// </summary>
    public bool TrackInventory { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item is active.
    /// </summary>
    public bool IsActive { get; set; }
}
