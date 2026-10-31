// <copyright file="ContactDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of a contact entity maintaining cross-domain compatibility.
/// </summary>
/// <param name="Id">The unique contact identifier.</param>
/// <param name="TenantId">The organizational tenant boundary identifier.</param>
/// <param name="Type">The contact category (e.g., Client, Supplier).</param>
/// <param name="Country">The two-letter ISO country code.</param>
/// <param name="CompanyName">The business or corporate entity name.</param>
/// <param name="FirstName">The first name of the contact person.</param>
/// <param name="LastName">The last name of the contact person.</param>
/// <param name="DisplayName">The formatted display name.</param>
/// <param name="Email">The contact electronic mail address.</param>
/// <param name="Phone">The telephone contact number.</param>
/// <param name="VatNumber">The value-added tax identifier.</param>
/// <param name="FiscalCode">The fiscal identification code.</param>
/// <param name="UidNumber">The Swiss enterprise identifier (UID).</param>
/// <param name="SdiCode">The Italian SDI recipient destination code.</param>
/// <param name="Iban">The bank account IBAN identifier.</param>
/// <param name="QrIban">The Swiss QR-IBAN identifier.</param>
/// <param name="City">The registered city or locality.</param>
/// <param name="AddressLine">The street and building address line.</param>
public record ContactDto(
    Guid Id,
    Guid TenantId,
    string Type,
    string Country,
    string CompanyName,
    string? FirstName,
    string? LastName,
    string DisplayName,
    string? Email,
    string? Phone,
    string? VatNumber,
    string? FiscalCode,
    string? UidNumber,
    string? SdiCode,
    string? Iban,
    string? QrIban,
    string? City,
    string? AddressLine);
