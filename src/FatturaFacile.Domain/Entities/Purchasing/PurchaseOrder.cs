using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Purchasing;

public sealed class PurchaseOrder : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid SupplierId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public DateTime? ExpectedDeliveryDate { get; private set; }
    public Money TotalAmount { get; private set; } = Money.Euro(0);
    public string Status { get; } = "Placed";

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private PurchaseOrder()
    {
    }

    public static PurchaseOrder Create(Guid tenantId, string orderNumber, Guid supplierId, Money total, DateTime? expectedDelivery = null)
    {
        return new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderNumber = orderNumber,
            SupplierId = supplierId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = total,
            ExpectedDeliveryDate = expectedDelivery,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}