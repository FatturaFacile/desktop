// <copyright file="TenantData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Administration;

/// <summary>
/// Data transfer object holding configuration and identification values required to create a new <see cref="Tenant"/>.
/// </summary>
/// <param name="Code">The unique organizational code for this tenant.</param>
/// <param name="BusinessName">The official legal business name.</param>
/// <param name="CountryCode">The primary country of operation ("IT" or "CH").</param>
/// <param name="BaseCurrency">The baseline operating currency for accounting ledgers.</param>
/// <param name="ContactEmail">The main contact email address.</param>
/// <param name="VatNumber">The VAT registration number (e.g., Italian Partita IVA or Swiss TVA number).</param>
/// <param name="FiscalCode">The Italian Codice Fiscale, if applicable.</param>
/// <param name="SdiCode">The 7-character Italian SDI routing code.</param>
/// <param name="SwissUid">The Swiss enterprise identifier (UID CHE-xxx.xxx.xxx).</param>
/// <param name="QrIban">The Swiss QR-IBAN for payment collection.</param>
public sealed record TenantData(
    string   Code,
    string   BusinessName,
    string   CountryCode,
    Currency BaseCurrency,
    string   ContactEmail,
    string?  VatNumber  = null,
    string?  FiscalCode = null,
    string?  SdiCode    = null,
    string?  SwissUid   = null,
    string?  QrIban     = null);
