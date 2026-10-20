using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Manufacturing;

/// <summary>
/// Represents production order.
/// </summary>
public sealed class ProductionOrder : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the pRD-2026/01.
    /// </summary>
    public string OrderNumber { get; private set; } = string.Empty;

/// <summary>
    /// Gets the bill of materials id.
    /// </summary>
    public Guid BillOfMaterialsId { get; private set; }

/// <summary>
    /// Gets the quantity to produce.
    /// </summary>
    public decimal QuantityToProduce { get; private set; }

/// <summary>
    /// Gets the quantity completed.
    /// </summary>
    public decimal QuantityCompleted { get; private set; }

/// <summary>
    /// Gets the scheduled start date.
    /// </summary>
    public DateTime ScheduledStartDate { get; private set; }

/// <summary>
    /// Gets the scheduled end date.
    /// </summary>
    public DateTime ScheduledEndDate { get; private set; }

/// <summary>
    /// Gets the status.
    /// </summary>
    public ProductionStatus Status { get; private set; } = ProductionStatus.Planned;

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

private ProductionOrder()
    {
    }

/// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="orderNumber">The order number.</param>
    /// <param name="bomId">The bom id.</param>
    /// <param name="quantity">The quantity.</param>
    /// <param name="start">The start.</param>
    /// <param name="end">The end.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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

/// <summary>
    /// Report Progress.
    /// </summary>
    /// <param name="qtyDone">The qty done.</param>
    public void ReportProgress(decimal qtyDone)
    {
        QuantityCompleted += qtyDone;
        Status = QuantityCompleted >= QuantityToProduce ? ProductionStatus.Completed : ProductionStatus.InProduction;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
