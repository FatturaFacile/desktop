using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

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

public sealed class Warehouse : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Address Address { get; private set; } = null!;
    public decimal TotalCapacitySquareMeters { get; private set; }

    private Warehouse()
    {
    }

    public static Warehouse Create(Guid tenantId, string code, string name, Address address, decimal capacity)
    {
        return new Warehouse
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Address = address,
            TotalCapacitySquareMeters = capacity,
        };
    }
}

public sealed class StockMovement : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid ItemCatalogId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public decimal Quantity { get; private set; }
    public MovementType MovementType { get; private set; }
    public DateTime TimestampUtc { get; private set; }
    public string? BatchNumber { get; private set; }
    public string? SerialNumber { get; private set; }
    public string? ReferenceDocument { get; private set; }

    private StockMovement()
    {
    }

    public static StockMovement Create(
        Guid tenantId,
        Guid itemId,
        Guid warehouseId,
        decimal quantity,
        MovementType type,
        string? batch = null,
        string? serial = null,
        string? refDoc = null)
    {
        return new StockMovement
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ItemCatalogId = itemId,
            WarehouseId = warehouseId,
            Quantity = quantity,
            MovementType = type,
            TimestampUtc = DateTime.UtcNow,
            BatchNumber = batch,
            SerialNumber = serial,
            ReferenceDocument = refDoc,
        };
    }
}

public sealed class TransportDocument : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    /// <summary>
    /// DDT-2026/001
    /// </summary>
    public string DocumentNumber { get; private set; } = string.Empty;
    public DateTime DocumentDate { get; private set; }
    public Guid ClientId { get; private set; }
    public Address DestinationAddress { get; private set; } = null!;
    /// <summary>
    /// Vendita, Conto Visione, Riparazione
    /// </summary>
    public string TransportationReason { get; private set; } = "Vendita";
    public string CarrierName { get; private set; } = "Mittente";
    public string GoodsAppearance { get; } = "Cartoni";
    public int NumberOfPackages { get; private set; } = 1;
    public decimal TotalWeightKg { get; private set; }
    /// <summary>
    /// Cross-border customs notes IT-CH
    /// </summary>
    public string? CustomsCmrNotes { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private TransportDocument()
    {
    }

    public static TransportDocument Create(
        Guid tenantId,
        string number,
        DateTime date,
        Guid clientId,
        Address destination,
        string reason,
        string carrier,
        int packages,
        decimal weightKg,
        string? customsCmr = null)
    {
        return new TransportDocument
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DocumentNumber = number,
            DocumentDate = date,
            ClientId = clientId,
            DestinationAddress = destination,
            TransportationReason = reason,
            CarrierName = carrier,
            NumberOfPackages = packages,
            TotalWeightKg = weightKg,
            CustomsCmrNotes = customsCmr,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
