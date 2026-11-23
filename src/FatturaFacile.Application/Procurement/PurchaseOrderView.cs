// <copyright file="PurchaseOrderView.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// A purchase order as read from the server.
/// </summary>
/// <param name="Id">The order identifier.</param>
/// <param name="Number">The order number.</param>
/// <param name="SupplierId">The supplier identifier.</param>
/// <param name="SupplierName">The supplier name, empty when not loaded.</param>
/// <param name="Status">The wire status (draft, approved, sent, confirmed, ...).</param>
/// <param name="OrderDate">The order date.</param>
/// <param name="ExpectedDeliveryDate">The requested delivery date.</param>
/// <param name="PromisedDeliveryDate">The delivery date promised by the supplier.</param>
/// <param name="Currency">The currency code.</param>
/// <param name="TotalAmount">The gross total.</param>
/// <param name="IsDropship">Whether the order ships straight to the customer.</param>
/// <param name="SalesOrderId">The linked sales order.</param>
/// <param name="ReminderCount">How many reminders were sent.</param>
/// <param name="LastReminderAt">When the last reminder was sent (UTC).</param>
/// <param name="SupplierConfirmationNumber">The number of the supplier confirmation.</param>
/// <param name="DispatchChannel">The channel of the last dispatch.</param>
/// <param name="Lines">The lines.</param>
public sealed record PurchaseOrderView(
    Guid Id,
    string Number,
    Guid? SupplierId,
    string SupplierName,
    string Status,
    DateOnly? OrderDate,
    DateOnly? ExpectedDeliveryDate,
    DateOnly? PromisedDeliveryDate,
    string Currency,
    decimal TotalAmount,
    bool IsDropship,
    Guid? SalesOrderId,
    int ReminderCount,
    DateTime? LastReminderAt,
    string? SupplierConfirmationNumber,
    string? DispatchChannel,
    IReadOnlyList<PurchaseOrderLineView> Lines);
