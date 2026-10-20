using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Inventory;

public sealed class ItemCatalog : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public string Code { get; private set; } = string.Empty;
    public string? BarcodeEan { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;

    /// <summary>
    /// PZ, KG, MT, ORA
    /// </summary>
    public string UnitOfMeasure { get; private set; } = "PZ";
    public decimal UnitCost { get; private set; }
    public decimal UnitSalePrice { get; private set; }
    public Currency Currency { get; private set; } = Currency.EUR;
    public decimal VatRate { get; private set; } = 22.0m;
    public decimal CurrentStockQuantity { get; private set; }
    public decimal MinimumStockAlert { get; } = 5;
    public bool IsSerialized { get; }
    public bool IsActive { get; } = true;

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private ItemCatalog()
    {
    }

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

    public void ApplyStockMovement(decimal quantityChange)
    {
        CurrentStockQuantity += quantityChange;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}