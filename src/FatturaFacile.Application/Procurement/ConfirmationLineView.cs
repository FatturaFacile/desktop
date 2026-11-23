// <copyright file="ConfirmationLineView.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// A line of a supplier confirmation with its variances against the order.
/// </summary>
/// <param name="PurchaseOrderLineId">The order line.</param>
/// <param name="OrderedQuantity">The ordered quantity.</param>
/// <param name="ConfirmedQuantity">The confirmed quantity.</param>
/// <param name="OrderedUnitPrice">The ordered unit price.</param>
/// <param name="ConfirmedUnitPrice">The confirmed unit price.</param>
/// <param name="RequestedDeliveryDate">The requested date.</param>
/// <param name="ConfirmedDeliveryDate">The confirmed date.</param>
/// <param name="QuantityVariance">The quantity variance in purchase units.</param>
/// <param name="QuantityVarianceBase">The quantity variance in base units.</param>
/// <param name="PriceVariance">The unit price variance.</param>
/// <param name="PriceVariancePercent">The unit price variance in percent.</param>
/// <param name="LineTotalVariance">The line total variance.</param>
/// <param name="DeliveryDelayDays">The days the confirmed date is after the requested one.</param>
/// <param name="VarianceReason">The reason given for the variance.</param>
public sealed record ConfirmationLineView(
    Guid PurchaseOrderLineId,
    decimal OrderedQuantity,
    decimal ConfirmedQuantity,
    decimal OrderedUnitPrice,
    decimal ConfirmedUnitPrice,
    DateOnly? RequestedDeliveryDate,
    DateOnly? ConfirmedDeliveryDate,
    decimal QuantityVariance,
    decimal QuantityVarianceBase,
    decimal PriceVariance,
    decimal PriceVariancePercent,
    decimal LineTotalVariance,
    int DeliveryDelayDays,
    string? VarianceReason);
