using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Manufacturing;

public sealed class ProductionOrder : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }

    /// <summary>
    /// PRD-2026/01
    /// </summary>
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid BillOfMaterialsId { get; private set; }
    public decimal QuantityToProduce { get; private set; }
    public decimal QuantityCompleted { get; private set; }
    public DateTime ScheduledStartDate { get; private set; }
    public DateTime ScheduledEndDate { get; private set; }
    public ProductionStatus Status { get; private set; } = ProductionStatus.Planned;

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private ProductionOrder()
    {
    }

    public static ProductionOrder Create(
        Guid tenantId,
        string orderNumber,
        Guid bomId,
        decimal quantity,
        DateTime start,
        DateTime end)
    {
        return new ProductionOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderNumber = orderNumber.Trim().ToUpperInvariant(),
            BillOfMaterialsId = bomId,
            QuantityToProduce = quantity,
            ScheduledStartDate = start,
            ScheduledEndDate = end,
            Status = ProductionStatus.Planned,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void ReportProgress(decimal qtyDone)
    {
        QuantityCompleted += qtyDone;
        Status = QuantityCompleted >= QuantityToProduce ? ProductionStatus.Completed : ProductionStatus.InProduction;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}