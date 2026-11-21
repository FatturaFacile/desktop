// <copyright file="ProductionOrder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Manufacturing;

/// <summary>
/// A production order, either pulled from the server or created offline.
/// </summary>
public class ProductionOrder : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the local identifier (equals the server identifier for pulled records).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the local tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the identifier assigned by the server (null while the creation is still queued).
    /// </summary>
    public Guid? ServerId { get; set; }

    /// <summary>
    /// Gets or sets the order number (a LOCAL-prefixed number while the creation is queued).
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the produced item identifier.
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Gets or sets the produced item code.
    /// </summary>
    public string ItemCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the produced item name.
    /// </summary>
    public string ItemName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the BOM.
    /// </summary>
    public Guid? BomId { get; set; }

    /// <summary>
    /// Gets or sets the routing.
    /// </summary>
    public Guid? RoutingId { get; set; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public ProductionOrderStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the quantity to produce.
    /// </summary>
    public decimal TargetQuantity { get; set; }

    /// <summary>
    /// Gets or sets the good quantity reported.
    /// </summary>
    public decimal CompletedQuantity { get; set; }

    /// <summary>
    /// Gets or sets the scrapped quantity reported.
    /// </summary>
    public decimal ScrappedQuantity { get; set; }

    /// <summary>
    /// Gets or sets the unit of measure.
    /// </summary>
    public string Uom { get; set; } = "PZ";

    /// <summary>
    /// Gets or sets the planned start date.
    /// </summary>
    public DateTime PlannedStartDate { get; set; }

    /// <summary>
    /// Gets or sets the planned end date.
    /// </summary>
    public DateTime PlannedEndDate { get; set; }

    /// <summary>
    /// Gets or sets the scheduling priority (lower is scheduled first).
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// Gets or sets the standard material cost of the order.
    /// </summary>
    public decimal StandardMaterialCost { get; set; }

    /// <summary>
    /// Gets or sets the standard labour cost of the order.
    /// </summary>
    public decimal StandardLaborCost { get; set; }

    /// <summary>
    /// Gets or sets the standard overhead cost of the order.
    /// </summary>
    public decimal StandardOverheadCost { get; set; }

    /// <summary>
    /// Gets or sets the material cost issued so far.
    /// </summary>
    public decimal ActualMaterialCost { get; set; }

    /// <summary>
    /// Gets or sets the labour cost posted so far.
    /// </summary>
    public decimal ActualLaborCost { get; set; }

    /// <summary>
    /// Gets or sets the overhead cost absorbed so far.
    /// </summary>
    public decimal ActualOverheadCost { get; set; }

    /// <summary>
    /// Gets or sets the standard value of the output received so far.
    /// </summary>
    public decimal ReceivedValue { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether unsynchronised local mutations exist (a pull never overwrites such an order).
    /// </summary>
    public bool HasPendingChanges { get; set; }

    /// <summary>
    /// Gets or sets the operations.
    /// </summary>
    public ICollection<ProductionOperation> Operations { get; set; } = [];

    /// <summary>
    /// Gets or sets the creation instant (UTC).
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last local modification instant (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the server updated_at stamp seen at the last pull.
    /// </summary>
    public string? ServerVersion { get; set; }
}
