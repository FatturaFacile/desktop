// <copyright file="PurchaseOrderDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of a procurement purchase order issued to a vendor or supplier.
/// </summary>
/// <param name="Id">The unique purchase order identifier.</param>
/// <param name="TenantId">The tenant organization boundary owning this purchase order.</param>
/// <param name="OrderNumber">The sequential procurement order reference code.</param>
/// <param name="SupplierId">The unique identifier of the designated vendor.</param>
/// <param name="SupplierName">The commercial or legal name of the vendor.</param>
/// <param name="OrderDate">The date when the order was placed or confirmed.</param>
/// <param name="TotalAmount">The total financial commitment amount of the order.</param>
/// <param name="Currency">The three-letter ISO currency code governing the purchase transaction.</param>
/// <param name="Status">The operational workflow status of the purchase order (e.g., Draft, Sent, Fulfilled, Canceled).</param>
public record PurchaseOrderDto(
    Guid     Id,
    Guid     TenantId,
    string   OrderNumber,
    Guid     SupplierId,
    string   SupplierName,
    DateTime OrderDate,
    decimal  TotalAmount,
    string   Currency,
    string   Status);
