// <copyright file="DropshipmentView.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// A dropshipment: supplier order shipped straight to the customer.
/// </summary>
/// <param name="Id">The dropshipment identifier.</param>
/// <param name="PurchaseOrderId">The supplier order.</param>
/// <param name="SalesOrderId">The sales order.</param>
/// <param name="Status">The wire status (pending, ddt_issued, partially_delivered, delivered).</param>
/// <param name="RecipientName">The recipient name.</param>
/// <param name="RecipientAddress">The recipient address on one line.</param>
/// <param name="DdtNumber">The number of the transport document.</param>
/// <param name="DdtIssuedAt">When the transport document was issued (UTC).</param>
/// <param name="SupplierShipmentReference">The shipment reference of the supplier.</param>
/// <param name="DeliveryProofReference">The proof of delivery reference.</param>
/// <param name="BillingAuthorized">Whether the customer may be invoiced.</param>
/// <param name="BillingAuthorizationReference">The billing authorisation reference.</param>
/// <param name="DeliveredAt">When it was delivered (UTC).</param>
/// <param name="Movements">The virtual stock movements.</param>
public sealed record DropshipmentView(
    Guid Id,
    Guid PurchaseOrderId,
    Guid? SalesOrderId,
    string Status,
    string RecipientName,
    string RecipientAddress,
    string? DdtNumber,
    DateTime? DdtIssuedAt,
    string? SupplierShipmentReference,
    string? DeliveryProofReference,
    bool BillingAuthorized,
    string? BillingAuthorizationReference,
    DateTime? DeliveredAt,
    IReadOnlyList<DropshipMovementView> Movements);
