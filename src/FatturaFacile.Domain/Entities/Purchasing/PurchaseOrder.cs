// <copyright file="PurchaseOrder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Purchasing;

/// <summary>
/// Represents purchase order.
/// </summary>
public sealed class PurchaseOrder : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private PurchaseOrder()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the order number.
    /// </summary>
    public string OrderNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the supplier id.
    /// </summary>
    public Guid SupplierId { get; private set; }

    /// <summary>
    /// Gets the order date.
    /// </summary>
    public DateTime OrderDate { get; private set; }

    /// <summary>
    /// Gets the expected delivery date.
    /// </summary>
    public DateTime? ExpectedDeliveryDate { get; private set; }

    /// <summary>
    /// Gets the total amount.
    /// </summary>
    public Money TotalAmount { get; private set; } = Money.Euro(0);

    /// <summary>
    /// Gets the status.
    /// </summary>
    public string Status { get; } = "Placed";

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

    /// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="orderNumber">The order number.</param>
    /// <param name="supplierId">The supplier id.</param>
    /// <param name="total">The total.</param>
    /// <param name="expectedDelivery">The expected delivery.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static PurchaseOrder Create(
        Guid tenantId,
        string orderNumber,
        Guid supplierId,
        Money total,
        DateTime? expectedDelivery = null)
    {
        return new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderNumber = orderNumber?.Trim() ?? string.Empty,
            SupplierId = supplierId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = total,
            ExpectedDeliveryDate = expectedDelivery,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
