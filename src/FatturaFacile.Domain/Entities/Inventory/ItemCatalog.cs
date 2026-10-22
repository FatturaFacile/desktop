// <copyright file="ItemCatalog.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Inventory;

/// <summary>
/// Represents an article, product, or stock-keeping unit (SKU) in the central merchandise catalog,
/// maintaining pricing, standard VAT rates, barcode identifiers, unit of measure, and on-hand stock levels.
/// </summary>
public sealed class ItemCatalog : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemCatalog"/> class for entity framework or serialization.
    /// </summary>
    private ItemCatalog()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization owning this catalog item.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique internal product code or SKU identifier.
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the European Article Number (EAN-13, EAN-8) or Universal Product Code (UPC) barcode string.
    /// </summary>
    public string? BarcodeEan { get; private set; }

    /// <summary>
    /// Gets the descriptive name and sales specification of the item.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the commercial classification group or category taxonomy of the product.
    /// </summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the standard commercial unit of measure (e.g., "PZ", "KG", "MT", "ORA").
    /// </summary>
    public string UnitOfMeasure { get; private set; } = "PZ";

    /// <summary>
    /// Gets the baseline purchase or manufacturing cost per unit.
    /// </summary>
    public decimal UnitCost { get; private set; }

    /// <summary>
    /// Gets the standard net selling price per unit before applicable taxes.
    /// </summary>
    public decimal UnitSalePrice { get; private set; }

    /// <summary>
    /// Gets the currency denomination used for cost and sales pricing.
    /// </summary>
    public Currency Currency { get; private set; } = Currency.EUR;

    /// <summary>
    /// Gets the standard statutory VAT tax rate percentage applicable to this item (e.g., 22.0% in Italy, 8.1% in Switzerland).
    /// </summary>
    public decimal VatRate { get; private set; } = 22.0m;

    /// <summary>
    /// Gets the current aggregated on-hand inventory quantity available across all warehouses.
    /// </summary>
    public decimal CurrentStockQuantity { get; private set; }

    /// <summary>
    /// Gets the reorder safety stock threshold that triggers restock or replenishment notifications.
    /// </summary>
    public decimal MinimumStockAlert { get; } = 5;

    /// <summary>
    /// Gets a value indicating whether individual instances of this product require serial number or lot batch tracking.
    /// </summary>
    public bool IsSerialized { get; }

    /// <summary>
    /// Gets a value indicating whether the item is currently active and available for quotation, purchase, or billing.
    /// </summary>
    public bool IsActive { get; } = true;

    /// <summary>
    /// Gets or sets the UTC timestamp when this item record was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier or username of the user who registered this item.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this item record was last updated.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier or username of the user who last updated this item record.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates a new catalog item entry with specified commercial, financial, and inventory attributes.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="code">The internal SKU or item reference code.</param>
    /// <param name="description">The commercial product description.</param>
    /// <param name="category">The product grouping category.</param>
    /// <param name="uom">The unit of measure (e.g., PZ, KG, MT).</param>
    /// <param name="unitCost">The baseline unit acquisition or production cost.</param>
    /// <param name="salePrice">The standard unit sales price excluding tax.</param>
    /// <param name="currency">The pricing currency denomination.</param>
    /// <param name="vatRate">The applicable statutory VAT percentage.</param>
    /// <param name="barcode">The optional EAN/UPC barcode string.</param>
    /// <returns>A newly created <see cref="ItemCatalog"/> aggregate instance.</returns>
    public static ItemCatalog Create(
        Guid     tenantId,
        string   code,
        string   description,
        string   category,
        string   uom,
        decimal  unitCost,
        decimal  salePrice,
        Currency currency,
        decimal  vatRate = 22.0m,
        string?  barcode = null)
    {
        return new ItemCatalog
               {
                   Id            = Guid.NewGuid(),
                   TenantId      = tenantId,
                   Code          = code?.Trim().ToUpperInvariant() ?? string.Empty,
                   Description   = description?.Trim()             ?? string.Empty,
                   Category      = category?.Trim()                ?? string.Empty,
                   UnitOfMeasure = uom?.Trim().ToUpperInvariant()  ?? string.Empty,
                   UnitCost      = unitCost,
                   UnitSalePrice = salePrice,
                   Currency      = currency,
                   VatRate       = vatRate,
                   BarcodeEan    = barcode?.Trim(),
                   CreatedAtUtc  = DateTime.UtcNow,
               };
    }

    /// <summary>
    /// Adjusts the physical on-hand inventory quantity following a verified inbound or outbound stock movement.
    /// </summary>
    /// <param name="quantityChange">The net delta to apply to the current stock level (positive for receipts, negative for issues).</param>
    public void ApplyStockMovement(decimal quantityChange)
    {
        CurrentStockQuantity += quantityChange;
        UpdatedAtUtc         =  DateTime.UtcNow;
    }

}
