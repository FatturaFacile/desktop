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
    /// <param name="data">The parameter object containing stock movement details.</param>
    /// <returns>A newly created <see cref="StockMovement"/> transaction record.</returns>
    public static StockMovement Create(
        StockMovementData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return new StockMovement
        {
            Id                = Guid.NewGuid(),
            TenantId          = data.TenantId,
            ItemCatalogId     = data.ItemId,
            WarehouseId       = data.WarehouseId,
            Quantity          = data.Quantity,
            MovementType      = data.Type,
            TimestampUtc      = DateTime.UtcNow,
            BatchNumber       = data.Batch,
            SerialNumber      = data.Serial,
            ReferenceDocument = data.RefDoc,
        };
    }

}
