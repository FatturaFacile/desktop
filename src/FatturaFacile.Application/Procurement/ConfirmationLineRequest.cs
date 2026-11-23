// <copyright file="ConfirmationLineRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// A line of a supplier confirmation to record.
/// </summary>
/// <param name="PurchaseOrderLineId">The order line.</param>
/// <param name="ConfirmedQuantity">The confirmed quantity.</param>
/// <param name="ConfirmedUnitPrice">The confirmed unit price.</param>
/// <param name="ConfirmedDeliveryDate">The confirmed delivery date.</param>
/// <param name="VarianceReason">The reason of a variance.</param>
public sealed record ConfirmationLineRequest(
    Guid PurchaseOrderLineId,
    decimal? ConfirmedQuantity,
    decimal? ConfirmedUnitPrice,
    DateOnly? ConfirmedDeliveryDate,
    string? VarianceReason);
