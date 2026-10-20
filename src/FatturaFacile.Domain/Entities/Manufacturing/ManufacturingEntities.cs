using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Manufacturing;

public sealed class BillOfMaterials : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public Guid FinishedProductItemId { get; private set; }
    public string BomCode { get; private set; } = string.Empty;
    public string RevisionNumber { get; private set; } = "1.0";
    public decimal BaseYieldPercentage { get; } = 100m;
    public bool IsActive { get; } = true;

    private readonly List<BomLine> _lines = [];
    public IReadOnlyCollection<BomLine> Lines => _lines.AsReadOnly();

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private BillOfMaterials()
    {
    }

    public static BillOfMaterials Create(Guid tenantId, Guid finishedItemId, string code, string revision = "1.0")
    {
        return new BillOfMaterials
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FinishedProductItemId = finishedItemId,
            BomCode = code.Trim().ToUpperInvariant(),
            RevisionNumber = revision.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void AddComponent(Guid componentItemId, decimal quantity, decimal scrapRate = 0m) => _lines.Add(new BomLine(Id, componentItemId, quantity, scrapRate));
}

public sealed class BomLine : Entity<Guid>
{
    public Guid BillOfMaterialsId { get; }
    public Guid ComponentItemId { get; }
    public decimal QuantityPerUnit { get; }
    public decimal ScrapRatePercentage { get; }

    internal BomLine(Guid bomId, Guid componentItemId, decimal quantity, decimal scrapRate)
    {
        Id = Guid.NewGuid();
        BillOfMaterialsId = bomId;
        ComponentItemId = componentItemId;
        QuantityPerUnit = quantity;
        ScrapRatePercentage = scrapRate;
    }

    private BomLine()
    {
    }
}

public sealed class WorkCenter : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public decimal HourlyMachineCost { get; private set; }
    public decimal CapacityHoursPerDay { get; private set; } = 8.0m;

    private WorkCenter()
    {
    }

    public static WorkCenter Create(Guid tenantId, string code, string name, decimal cost, decimal capacityHours = 8.0m)
    {
        return new WorkCenter
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            HourlyMachineCost = cost,
            CapacityHoursPerDay = capacityHours,
        };
    }
}

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
