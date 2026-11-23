// <copyright file="DropshipMovementView.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// A virtual stock movement of a dropshipment.
/// </summary>
/// <param name="PurchaseOrderLineId">The order line.</param>
/// <param name="Direction">The direction (in or out).</param>
/// <param name="QuantityBase">The quantity in base units.</param>
/// <param name="BaseUom">The base unit.</param>
/// <param name="UnitCostBase">The cost per base unit.</param>
/// <param name="ValuedAmount">The valued amount.</param>
/// <param name="MovedAt">When it moved (UTC).</param>
public sealed record DropshipMovementView(
    Guid PurchaseOrderLineId,
    string Direction,
    decimal QuantityBase,
    string BaseUom,
    decimal UnitCostBase,
    decimal ValuedAmount,
    DateTime MovedAt);
