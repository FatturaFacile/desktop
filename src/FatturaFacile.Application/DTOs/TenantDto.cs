using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Represents tenant dto.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="Code">The code.</param>
/// <param name="BusinessName">The business name.</param>
/// <param name="CountryCode">The country code.</param>
/// <param name="BaseCurrency">The base currency.</param>
/// <param name="VatNumber">The vat number.</param>
/// <param name="FiscalCode">The fiscal code.</param>
/// <param name="SdiCode">The sdi code.</param>
/// <param name="SwissUid">The swiss uid.</param>
/// <param name="QrIban">The qr iban.</param>
/// <param name="ContactEmail">The contact email.</param>
/// <param name="IsActive">The is active.</param>
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
