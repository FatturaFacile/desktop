using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Represents client dto.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="TenantId">The tenant id.</param>
/// <param name="BusinessName">The business name.</param>
/// <param name="TradeName">The trade name.</param>
/// <param name="Jurisdiction">The jurisdiction.</param>
/// <param name="FiscalCode">The fiscal code.</param>
/// <param name="VatNumber">The vat number.</param>
/// <param name="SdiCode">The sdi code.</param>
/// <param name="PecEmail">The pec email.</param>
/// <param name="SwissUid">The swiss uid.</param>
/// <param name="QrIban">The qr iban.</param>
/// <param name="CantonalCode">The cantonal code.</param>
/// <param name="AddressFormatted">The address formatted.</param>
/// <param name="Email">The email.</param>
/// <param name="PhoneNumber">The phone number.</param>
/// <param name="PaymentTermDays">The payment term days.</param>
/// <param name="CreditLimitAmount">The credit limit amount.</param>
/// <param name="CreditLimitCurrency">The credit limit currency.</param>
/// <param name="IsActive">The is active.</param>
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
