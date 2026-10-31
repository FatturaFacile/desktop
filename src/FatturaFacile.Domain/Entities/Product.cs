// <copyright file="Product.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents a billable good, physical inventory item, or service in the tenant product catalog.
/// </summary>
public class Product : EntityBase, IMustHaveTenant, ISoftDeletable
{
    /// <inheritdoc />
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the stock keeping unit (SKU) or internal article code.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the European Article Number (EAN), UPC, or barcode string.
    /// </summary>
    public string? Barcode { get; set; }

    /// <summary>
    /// Gets or sets the product or service commercial title.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets detailed product description or technical specifications.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the unit of measure (e.g. "PZ", "KG", "H", "M").
    /// </summary>
    public string UnitMeasure { get; set; } = "PZ";

    /// <summary>
    /// Gets or sets the wholesale acquisition or manufacturing cost price.
    /// </summary>
    public decimal CostPrice { get; set; }

    /// <summary>
    /// Gets or sets the default retail or net sales price before taxes.
    /// </summary>
    public decimal SalePrice { get; set; }

    /// <summary>
    /// Gets or sets the default applicable VAT tax percentage rate.
    /// </summary>
    public decimal TaxRate { get; set; } = 8.1m;

    /// <summary>
    /// Gets or sets a value indicating whether warehouse stock movements and inventory tracking are enabled.
    /// </summary>
    public bool TrackInventory { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the product is currently active and selectable on new invoices.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <inheritdoc />
    public bool IsDeleted { get; set; }

    /// <inheritdoc />
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the navigation property for the owning tenant organization.
    /// </summary>
    public Tenant? Tenant { get; set; }
}
