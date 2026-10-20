using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

public record SupplierDto(
    Guid Id,
    Guid TenantId,
    string Name,
    Jurisdiction Jurisdiction,
    string? VatNumber,
    string? SwissUid,
    string? SdiCode,
    string AddressFormatted,
    string? Iban,
    int PaymentTermsDays,
    bool IsActive);