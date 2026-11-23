// <copyright file="SupplierConfirmationView.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// A supplier order confirmation.
/// </summary>
/// <param name="Id">The confirmation identifier.</param>
/// <param name="PurchaseOrderId">The order.</param>
/// <param name="ConfirmationNumber">The number given by the supplier.</param>
/// <param name="HasVariance">Whether it differs from the order.</param>
/// <param name="RequiresReview">Whether a buyer has to review it.</param>
/// <param name="Notes">The notes.</param>
/// <param name="ConfirmedAt">The date of the confirmation.</param>
/// <param name="PriceVarianceTotal">The total price variance.</param>
/// <param name="QuantityVarianceTotal">The total quantity variance.</param>
/// <param name="Lines">The lines.</param>
public sealed record SupplierConfirmationView(
    Guid Id,
    Guid PurchaseOrderId,
    string ConfirmationNumber,
    bool HasVariance,
    bool RequiresReview,
    string? Notes,
    DateOnly ConfirmedAt,
    decimal PriceVarianceTotal,
    decimal QuantityVarianceTotal,
    IReadOnlyList<ConfirmationLineView> Lines);
