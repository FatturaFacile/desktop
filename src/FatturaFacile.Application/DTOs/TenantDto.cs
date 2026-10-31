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
    bool     IsActive)
{
    /// <summary>
    /// Gets the tenant display name.
    /// </summary>
    public string Name => BusinessName;

    /// <summary>
    /// Gets the legal registered corporate name.
    /// </summary>
    public string? LegalName => BusinessName;

    /// <summary>
    /// Gets the Swiss enterprise identifier (UID) number.
    /// </summary>
    public string? UidNumber => SwissUid;

    /// <summary>
    /// Gets the two-letter ISO country code.
    /// </summary>
    public string Country => CountryCode;

    /// <summary>
    /// Gets the default operating base currency as a string.
    /// </summary>
    public string DefaultCurrency => BaseCurrency.ToString();

    /// <summary>
    /// Gets the primary bank IBAN account identifier.
    /// </summary>
    public string? Iban { get; init; }

    /// <summary>
    /// Gets the PEC certified electronic mail address.
    /// </summary>
    public string? PecEmail => ContactEmail;

    /// <summary>
    /// Gets the two-letter uppercase initials derived from the tenant name.
    /// </summary>
    public string Initials => string.IsNullOrWhiteSpace(Name)
        ? "??"
        : string.Join("", Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(w => char.ToUpperInvariant(w[0])));

    /// <summary>
    /// Gets the primary fiscal or VAT identification string formatted for UI display.
    /// </summary>
    public string TaxIdDisplay => !string.IsNullOrWhiteSpace(VatNumber)
        ? VatNumber
        : (!string.IsNullOrWhiteSpace(UidNumber) ? UidNumber : (FiscalCode ?? "N/A"));

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantDto"/> record with detailed parameters.
    /// </summary>
    /// <param name="id">The unique tenant identifier.</param>
    /// <param name="name">The business or corporate name.</param>
    /// <param name="legalName">The registered legal corporate name.</param>
    /// <param name="fiscalCode">The fiscal identifier code.</param>
    /// <param name="vatNumber">The VAT registration number.</param>
    /// <param name="uidNumber">The Swiss enterprise identifier.</param>
    /// <param name="country">The country code.</param>
    /// <param name="defaultCurrency">The base currency string representation.</param>
    /// <param name="iban">The primary IBAN account.</param>
    /// <param name="qrIban">The Swiss QR-IBAN account.</param>
    /// <param name="sdiCode">The Italian SDI recipient destination code.</param>
    /// <param name="pecEmail">The certified electronic mail address.</param>
    public TenantDto(
        Guid id,
        string name,
        string? legalName,
        string? fiscalCode,
        string? vatNumber,
        string? uidNumber,
        string country,
        string defaultCurrency,
        string? iban,
        string? qrIban,
        string? sdiCode,
        string? pecEmail)
        : this(
            id,
            name,
            legalName ?? name,
            country,
            Enum.TryParse<Currency>(defaultCurrency, true, out var c) ? c : Currency.EUR,
            vatNumber,
            fiscalCode,
            sdiCode,
            uidNumber,
            qrIban,
            pecEmail ?? string.Empty,
            true)
    {
        Iban = iban;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantDto"/> record with default values.
    /// </summary>
    public TenantDto()
        : this(default, string.Empty, string.Empty, "CH", Currency.CHF, null, null, null, null, null, string.Empty, true)
    {
    }
}
