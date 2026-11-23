// <copyright file="PurchaseOrderLineView.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// A purchase order line as read from the server, with the dual unit of measure and the cascading discount.
/// </summary>
/// <param name="Id">The line identifier.</param>
/// <param name="LineNumber">The line number.</param>
/// <param name="Description">The description.</param>
/// <param name="QuantityOrdered">The ordered quantity in the purchase unit.</param>
/// <param name="QuantityReceived">The received quantity in the purchase unit.</param>
/// <param name="Uom">The purchase unit of measure.</param>
/// <param name="BaseUom">The stock (base) unit of measure.</param>
/// <param name="UomConversionFactor">How many base units one purchase unit holds.</param>
/// <param name="QuantityBaseOrdered">The ordered quantity in base units.</param>
/// <param name="QuantityBaseReceived">The received quantity in base units.</param>
/// <param name="QuantityConfirmed">The quantity confirmed by the supplier, when known.</param>
/// <param name="DiscountCascade">The cascading discount steps in percent, in order.</param>
/// <param name="DiscountPercentEffective">The effective discount of the cascade in percent.</param>
/// <param name="PromisedDeliveryDate">The delivery date promised by the supplier.</param>
/// <param name="ConfirmedDeliveryDate">The delivery date confirmed by the supplier.</param>
/// <param name="UnitPrice">The list unit price.</param>
/// <param name="LineNetAmount">The net amount of the line.</param>
public sealed record PurchaseOrderLineView(
    Guid Id,
    int LineNumber,
    string Description,
    decimal QuantityOrdered,
    decimal QuantityReceived,
    string Uom,
    string BaseUom,
    decimal UomConversionFactor,
    decimal QuantityBaseOrdered,
    decimal QuantityBaseReceived,
    decimal? QuantityConfirmed,
    IReadOnlyList<decimal> DiscountCascade,
    decimal DiscountPercentEffective,
    DateOnly? PromisedDeliveryDate,
    DateOnly? ConfirmedDeliveryDate,
    decimal UnitPrice,
    decimal LineNetAmount);
