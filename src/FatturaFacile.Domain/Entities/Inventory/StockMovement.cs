// <copyright file="StockMovement.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Inventory;

/// <summary>
/// Represents stock movement.
/// </summary>
public sealed class StockMovement : Entity<Guid>, ITenantEntity
{
    private StockMovement()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the item catalog id.
    /// </summary>
    public Guid ItemCatalogId { get; private set; }

    /// <summary>
    /// Gets the warehouse id.
    /// </summary>
    public Guid WarehouseId { get; private set; }

    /// <summary>
    /// Gets the quantity.
    /// </summary>
    public decimal Quantity { get; private set; }

    /// <summary>
    /// Gets the movement type.
    /// </summary>
    public MovementType MovementType { get; private set; }

    /// <summary>
    /// Gets the timestamp utc.
    /// </summary>
    public DateTime TimestampUtc { get; private set; }

    /// <summary>
    /// Gets the batch number.
    /// </summary>
    public string? BatchNumber { get; private set; }

    /// <summary>
    /// Gets the serial number.
    /// </summary>
    public string? SerialNumber { get; private set; }

    /// <summary>
    /// Gets the reference document.
    /// </summary>
    public string? ReferenceDocument { get; private set; }

    /// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="itemId">The item id.</param>
    /// <param name="warehouseId">The warehouse id.</param>
    /// <param name="quantity">The quantity.</param>
    /// <param name="type">The type.</param>
    /// <param name="batch">The batch.</param>
    /// <param name="serial">The serial.</param>
    /// <param name="refDoc">The ref doc.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static StockMovement Create(
        Guid tenantId,
        Guid itemId,
        Guid warehouseId,
        decimal quantity,
        MovementType type,
        string? batch = null,
        string? serial = null,
        string? refDoc = null)
    {
        return new StockMovement
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ItemCatalogId = itemId,
            WarehouseId = warehouseId,
            Quantity = quantity,
            MovementType = type,
            TimestampUtc = DateTime.UtcNow,
            BatchNumber = batch,
            SerialNumber = serial,
            ReferenceDocument = refDoc,
        };
    }
}
