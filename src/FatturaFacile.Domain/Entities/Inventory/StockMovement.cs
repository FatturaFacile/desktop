using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Inventory;

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