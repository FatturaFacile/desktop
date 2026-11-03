// <copyright file="StockMovementData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Inventory;

/// <summary>
/// Data transfer object holding item, warehouse, batch, and quantity details for a new stock ledger movement.
/// </summary>
/// <param name="TenantId">The unique tenant identifier.</param>
/// <param name="ItemId">The unique catalog item identifier.</param>
/// <param name="WarehouseId">The source or destination warehouse identifier.</param>
/// <param name="Quantity">The quantity transacted.</param>
/// <param name="Type">The operational transaction type (e.g., Inbound, Outbound, Scrap, Adjustment).</param>
/// <param name="Batch">The optional lot or manufacturing batch number.</param>
/// <param name="Serial">The optional equipment hardware serial number.</param>
/// <param name="RefDoc">The originating document reference (e.g., DDT or invoice number).</param>
public sealed record StockMovementData(
    Guid         TenantId,
    Guid         ItemId,
    Guid         WarehouseId,
    decimal      Quantity,
    MovementType Type,
    string?      Batch  = null,
    string?      Serial = null,
    string?      RefDoc = null);
