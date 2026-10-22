// <copyright file="PurchaseOrder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Purchasing;

/// <summary>
/// Represents a formal purchase order aggregate issued to a vendor or supplier,
/// documenting committed purchase amounts, order status, and anticipated delivery schedules.
/// </summary>
public sealed class PurchaseOrder : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{

    /// <summary>
    /// Initializes a new instance of the <see cref="PurchaseOrder"/> class for entity framework or serialization.
    /// </summary>
    private PurchaseOrder()
    {
    }

    /// <summary>
    /// Gets or sets the unique tenant identifier for multi-tenant data partitioning.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique purchase order reference code (e.g. "PO-2026/089").
    /// </summary>
    public string OrderNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the unique identifier of the target supplier receiving the order.
    /// </summary>
    public Guid SupplierId { get; private set; }

    /// <summary>
    /// Gets the date and time when the purchase order was generated and transmitted to the supplier.
    /// </summary>
    public DateTime OrderDate { get; private set; }

    /// <summary>
    /// Gets the anticipated delivery or fulfillment date promised by or requested from the supplier, if specified.
    /// </summary>
    public DateTime? ExpectedDeliveryDate { get; private set; }

    /// <summary>
    /// Gets the committed total monetary amount and currency for the purchase order.
    /// </summary>
    public Money TotalAmount { get; private set; } = Money.Euro(amount: 0);

    /// <summary>
    /// Gets the current procurement lifecycle state of the purchase order (defaults to "Placed").
    /// </summary>
    public string Status { get; } = "Placed";

    /// <summary>
    /// Gets or sets the UTC timestamp when the purchase order record was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the user identifier or system process that created the purchase order.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the purchase order was last updated.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the user identifier or system process that last updated the order.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates a new <see cref="PurchaseOrder"/> aggregate initialized in the "Placed" status.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="orderNumber">The formal reference code for the purchase order.</param>
    /// <param name="supplierId">The supplier entity identifier.</param>
    /// <param name="total">The total monetary value committed in this order.</param>
    /// <param name="expectedDelivery">The optional scheduled or requested delivery date.</param>
    /// <returns>A newly instantiated <see cref="PurchaseOrder"/> aggregate.</returns>
    public static PurchaseOrder Create(
        Guid      tenantId,
        string    orderNumber,
        Guid      supplierId,
        Money     total,
        DateTime? expectedDelivery = null)
    {
        return new PurchaseOrder
               {
                   Id                   = Guid.NewGuid(),
                   TenantId             = tenantId,
                   OrderNumber          = orderNumber?.Trim() ?? string.Empty,
                   SupplierId           = supplierId,
                   OrderDate            = DateTime.UtcNow,
                   TotalAmount          = total,
                   ExpectedDeliveryDate = expectedDelivery,
                   CreatedAtUtc         = DateTime.UtcNow,
               };
    }

}
