// <copyright file="DropshipLineRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// A sales order line to order from the supplier.
/// </summary>
/// <param name="SalesOrderItemId">The sales order line.</param>
/// <param name="UnitPrice">The purchase unit price, when it differs from the catalogue.</param>
/// <param name="Uom">The purchase unit.</param>
/// <param name="DiscountCascade">The cascading discount steps in percent.</param>
public sealed record DropshipLineRequest(
    Guid SalesOrderItemId,
    decimal? UnitPrice,
    string? Uom,
    IReadOnlyList<decimal> DiscountCascade);
