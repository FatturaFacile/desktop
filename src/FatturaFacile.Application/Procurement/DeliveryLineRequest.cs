// <copyright file="DeliveryLineRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// A delivered quantity of a dropshipment line.
/// </summary>
/// <param name="PurchaseOrderLineId">The order line.</param>
/// <param name="QuantityDelivered">The quantity delivered in the purchase unit.</param>
public sealed record DeliveryLineRequest(
    Guid PurchaseOrderLineId,
    decimal QuantityDelivered);
