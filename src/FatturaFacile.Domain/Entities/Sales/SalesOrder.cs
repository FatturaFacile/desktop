using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Sales;

public sealed class SalesOrder : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid ClientId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public Currency Currency { get; private set; } = Currency.EUR;
    public decimal TotalAmount { get; }
    public string Status { get; } = "Confirmed";

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private SalesOrder()
    {
    }

    public static SalesOrder Create(Guid tenantId, string orderNumber, Guid clientId, Currency currency)
    {
        return new SalesOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderNumber = orderNumber,
            ClientId = clientId,
            OrderDate = DateTime.UtcNow,
            Currency = currency,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}