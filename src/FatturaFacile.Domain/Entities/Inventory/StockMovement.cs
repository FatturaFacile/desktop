// <copyright file="StockMovement.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Inventory;

/// <summary>
/// Represents an immutable inventory stock ledger transaction recording the physical transfer, receipt,
/// issuance, scrap write-off, or inventory reconciliation adjustment of merchandise, complete with batch and serial traceability.
/// </summary>
public sealed class StockMovement : Entity<Guid>, ITenantEntity
{

    /// <summary>
    /// Initializes a new instance of the <see cref="StockMovement"/> class for entity framework or serialization.
    /// </summary>
    private StockMovement()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization owning this stock movement transaction.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique identifier of the catalog item being moved.
    /// </summary>
    public Guid ItemCatalogId { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the warehouse facility where the stock adjustment occurs.
    /// </summary>
    public Guid WarehouseId { get; private set; }

    /// <summary>
    /// Gets the physical quantity of goods moved, expressed in the item's standard unit of measure.
    /// </summary>
    public decimal Quantity { get; private set; }

    /// <summary>
    /// Gets the operational classification of the movement (e.g., InboundPurchase, OutboundSale, Transfer, Adjustment, Scrap).
    /// </summary>
    public MovementType MovementType { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the stock ledger entry was posted.
    /// </summary>
    public DateTime TimestampUtc { get; private set; }

    /// <summary>
    /// Gets the production lot or manufacturing batch tracking code associated with perishable or regulated goods.
    /// </summary>
    public string? BatchNumber { get; private set; }

    /// <summary>
    /// Gets the unique equipment or item hardware serial number, if serialized inventory tracking applies.
    /// </summary>
    public string? SerialNumber { get; private set; }

    /// <summary>
    /// Gets the originating source document reference (e.g., DDT number, supplier purchase invoice, sales order, or stocktake sheet).
    /// </summary>
    public string? ReferenceDocument { get; private set; }

    /// <summary>
    /// Records a new immutable stock ledger transaction for an inventory item within a designated warehouse.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="itemId">The unique catalog item identifier.</param>
    /// <param name="warehouseId">The source or destination warehouse identifier.</param>
    /// <param name="quantity">The quantity transacted.</param>
    /// <param name="type">The operational transaction type (e.g., Inbound, Outbound, Scrap, Adjustment).</param>
    /// <param name="batch">The optional lot or manufacturing batch number.</param>
    /// <param name="serial">The optional equipment hardware serial number.</param>
    /// <param name="refDoc">The originating document reference (e.g., DDT or invoice number).</param>
    /// <returns>A newly created <see cref="StockMovement"/> transaction record.</returns>
    public static StockMovement Create(
        Guid         tenantId,
        Guid         itemId,
        Guid         warehouseId,
        decimal      quantity,
        MovementType type,
        string?      batch  = null,
        string?      serial = null,
        string?      refDoc = null)
    {
        return new StockMovement
               {
                   Id                = Guid.NewGuid(),
                   TenantId          = tenantId,
                   ItemCatalogId     = itemId,
                   WarehouseId       = warehouseId,
                   Quantity          = quantity,
                   MovementType      = type,
                   TimestampUtc      = DateTime.UtcNow,
                   BatchNumber       = batch,
                   SerialNumber      = serial,
                   ReferenceDocument = refDoc,
               };
    }

}
