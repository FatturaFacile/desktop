// <copyright file="MovementType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies the nature of an inventory stock movement impacting warehouse balances and valuation.
/// </summary>
public enum MovementType
{
    /// <summary>
    /// Inbound stock receipt originating from supplier deliveries, purchase orders, customer returns, or finished goods production intake.
    /// </summary>
    Receipt = 1,

    /// <summary>
    /// Outbound stock issue for customer deliveries, sales shipments, work-order consumption, or scrap disposal.
    /// </summary>
    Issue = 2,

    /// <summary>
    /// Internal transfer of items between distinct physical warehouses, storage locations, or logistical bins within the organization.
    /// </summary>
    Transfer = 3,

    /// <summary>
    /// Inventory quantity or valuation correction resulting from physical cycle counts, periodic stock reconciliations, loss, or spoilage.
    /// </summary>
    Adjustment = 4,
}
