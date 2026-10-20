namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Represents purchase order dto.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="TenantId">The tenant id.</param>
/// <param name="OrderNumber">The order number.</param>
/// <param name="SupplierId">The supplier id.</param>
/// <param name="SupplierName">The supplier name.</param>
/// <param name="OrderDate">The order date.</param>
/// <param name="TotalAmount">The total amount.</param>
/// <param name="Currency">The currency.</param>
/// <param name="Status">The status.</param>
public record PurchaseOrderDto(
    Guid Id,
    Guid TenantId,
    string OrderNumber,
    Guid SupplierId,
    string SupplierName,
    DateTime OrderDate,
    decimal TotalAmount,
    string Currency,
    string Status);
