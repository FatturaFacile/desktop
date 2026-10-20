namespace FatturaFacile.Application.DTOs;

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