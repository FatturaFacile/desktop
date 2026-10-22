// <copyright file="ClientDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of a customer or business client profile.
/// </summary>
/// <param name="Id">The unique client identifier.</param>
/// <param name="TenantId">The tenant organization boundary owning this client profile.</param>
/// <param name="BusinessName">The primary registered corporate name or legal business designation.</param>
/// <param name="TradeName">An optional commercial trading brand or operating name.</param>
/// <param name="Jurisdiction">The fiscal and legal operational jurisdiction (Italy or Switzerland).</param>
/// <param name="FiscalCode">The Italian tax identification code (Codice Fiscale), if applicable.</param>
/// <param name="VatNumber">The value-added tax identification number (Partita IVA / UID), if registered.</param>
/// <param name="SdiCode">The 7-character recipient code (Codice Destinatario) for Italian electronic invoicing.</param>
/// <param name="PecEmail">The certified electronic mail address (Posta Elettronica Certificata) for Italian invoicing.</param>
/// <param name="SwissUid">The Swiss business identification number (Unternehmens-Identifikationsnummer - UID).</param>
/// <param name="QrIban">The special QR-IBAN used for structured Swiss QR-bill receivables.</param>
/// <param name="CantonalCode">The two-letter Swiss canton abbreviation (e.g., "TI", "ZH"), where applicable.</param>
/// <param name="AddressFormatted">The fully formatted billing street and postal address.</param>
/// <param name="Email">The primary electronic mail contact address.</param>
/// <param name="PhoneNumber">The telephone contact number.</param>
/// <param name="PaymentTermDays">The agreed commercial payment terms, in days from invoice issuance.</param>
/// <param name="CreditLimitAmount">The maximum commercial credit line authorized for this client.</param>
/// <param name="CreditLimitCurrency">The currency code governing the allocated credit limit (e.g., "EUR", "CHF").</param>
/// <param name="IsActive">A flag indicating whether the client relationship is active and available for billing operations.</param>
public record ClientDto(
    Guid         Id,
    Guid         TenantId,
    string       BusinessName,
    string?      TradeName,
    Jurisdiction Jurisdiction,
    string?      FiscalCode,
    string?      VatNumber,
    string?      SdiCode,
    string?      PecEmail,
    string?      SwissUid,
    string?      QrIban,
    string?      CantonalCode,
    string       AddressFormatted,
    string       Email,
    string?      PhoneNumber,
    int          PaymentTermDays,
    decimal      CreditLimitAmount,
    string       CreditLimitCurrency,
    bool         IsActive);
