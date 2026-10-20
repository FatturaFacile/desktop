using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

public record TenantDto(
    Guid Id,
    string Code,
    string BusinessName,
    string CountryCode,
    Currency BaseCurrency,
    string? VatNumber,
    string? FiscalCode,
    string? SdiCode,
    string? SwissUid,
    string? QrIban,
    string ContactEmail,
    bool IsActive);