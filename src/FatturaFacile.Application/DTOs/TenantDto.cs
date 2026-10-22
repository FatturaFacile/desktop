// <copyright file="TenantDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of an organizational tenant boundary and fiscal profile.
/// </summary>
/// <param name="Id">The unique tenant identifier.</param>
/// <param name="Code">The unique alphanumeric identifier code assigned to the tenant.</param>
/// <param name="BusinessName">The legal corporate registered business name of the tenant organization.</param>
/// <param name="CountryCode">The two-letter ISO country code governing the tenant's primary fiscal residency (e.g., "IT", "CH").</param>
/// <param name="BaseCurrency">The default operating base accounting currency configured for the tenant.</param>
/// <param name="VatNumber">The value-added tax identifier, if registered.</param>
/// <param name="FiscalCode">The fiscal tax code for Italian corporate entities, if applicable.</param>
/// <param name="SdiCode">The SDI recipient code for Italian B2B electronic billing.</param>
/// <param name="SwissUid">The Swiss enterprise identifier (UID), if applicable.</param>
/// <param name="QrIban">The Swiss QR-IBAN account associated with tenant receivables.</param>
/// <param name="ContactEmail">The administrative contact electronic mail address.</param>
/// <param name="IsActive">A flag indicating whether tenant operations and user logins are permitted.</param>
public record TenantDto(
    Guid     Id,
    string   Code,
    string   BusinessName,
    string   CountryCode,
    Currency BaseCurrency,
    string?  VatNumber,
    string?  FiscalCode,
    string?  SdiCode,
    string?  SwissUid,
    string?  QrIban,
    string   ContactEmail,
    bool     IsActive);
