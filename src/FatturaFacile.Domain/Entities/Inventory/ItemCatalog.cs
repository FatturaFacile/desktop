using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Inventory;

/// <summary>
/// Represents item catalog.
/// </summary>
public sealed class ItemCatalog : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the code.
    /// </summary>
    public string Code { get; private set; } = string.Empty;

/// <summary>
    /// Gets the barcode ean.
    /// </summary>
    public string? BarcodeEan { get; private set; }

/// <summary>
    /// Gets the description.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

/// <summary>
    /// Gets the category.
    /// </summary>
    public string Category { get; private set; } = string.Empty;

/// <summary>
    /// Gets the pZ, KG, MT, ORA.
    /// </summary>
    public string UnitOfMeasure { get; private set; } = "PZ";

/// <summary>
    /// Gets the unit cost.
    /// </summary>
    public decimal UnitCost { get; private set; }

/// <summary>
    /// Gets the unit sale price.
    /// </summary>
    public decimal UnitSalePrice { get; private set; }

/// <summary>
    /// Gets the currency.
    /// </summary>
    public Currency Currency { get; private set; } = Currency.EUR;

/// <summary>
    /// Gets the vat rate.
    /// </summary>
    public decimal VatRate { get; private set; } = 22.0m;

/// <summary>
    /// Gets the current stock quantity.
    /// </summary>
    public decimal CurrentStockQuantity { get; private set; }

/// <summary>
    /// Gets the minimum stock alert.
    /// </summary>
    public decimal MinimumStockAlert { get; } = 5;

/// <summary>
    /// Gets a value indicating whether serialized.
    /// </summary>
    public bool IsSerialized { get; }

/// <summary>
    /// Gets a value indicating whether active.
    /// </summary>
    public bool IsActive { get; } = true;

/// <summary>
    /// Gets or sets the created at utc.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

/// <summary>
    /// Gets or sets the created by.
    /// </summary>
    public string? CreatedBy { get; set; }

/// <summary>
    /// Gets or sets the updated at utc.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

/// <summary>
    /// Gets or sets the updated by.
    /// </summary>
    public string? UpdatedBy { get; set; }

private ItemCatalog()
    {
    }

/// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="code">The code.</param>
    /// <param name="description">The description.</param>
    /// <param name="category">The category.</param>
    /// <param name="uom">The uom.</param>
    /// <param name="unitCost">The unit cost.</param>
    /// <param name="salePrice">The sale price.</param>
    /// <param name="currency">The currency.</param>
    /// <param name="vatRate">The vat rate.</param>
    /// <param name="barcode">The barcode.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static ItemCatalog Create(
        Guid tenantId,
        string code,
        string description,
        string category,
        string uom,
        decimal unitCost,
        decimal salePrice,
        Currency currency,
        decimal vatRate = 22.0m,
        string? barcode = null)
    {
        return new ItemCatalog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = code.Trim().ToUpperInvariant(),
            Description = description.Trim(),
            Category = category.Trim(),
            UnitOfMeasure = uom.Trim().ToUpperInvariant(),
            UnitCost = unitCost,
            UnitSalePrice = salePrice,
            Currency = currency,
            VatRate = vatRate,
            BarcodeEan = barcode?.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

/// <summary>
    /// Apply Stock Movement.
    /// </summary>
    /// <param name="quantityChange">The quantity change.</param>
    public void ApplyStockMovement(decimal quantityChange)
    {
        CurrentStockQuantity += quantityChange;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
