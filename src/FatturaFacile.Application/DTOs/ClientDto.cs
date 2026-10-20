using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

public record ClientDto(
    Guid Id,
    Guid TenantId,
    string BusinessName,
    string? TradeName,
    Jurisdiction Jurisdiction,
    string? FiscalCode,
    string? VatNumber,
    string? SdiCode,
    string? PecEmail,
    string? SwissUid,
    string? QrIban,
    string? CantonalCode,
    string AddressFormatted,
    string Email,
    string? PhoneNumber,
    int PaymentTermDays,
    decimal CreditLimitAmount,
    string CreditLimitCurrency,
    bool IsActive);