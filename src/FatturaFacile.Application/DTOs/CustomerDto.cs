// <copyright file="CustomerDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of a customer contact record containing billing and fiscal attributes.
/// </summary>
/// <param name="Id">The unique customer contact identifier.</param>
/// <param name="TenantId">The organizational tenant identifier scope.</param>
/// <param name="Type">The contact category (e.g., Company, Individual).</param>
/// <param name="Country">The two-letter ISO country code governing billing tax rules.</param>
/// <param name="CompanyName">The legal corporate company name, if applicable.</param>
/// <param name="FirstName">The first name of the individual customer.</param>
/// <param name="LastName">The last name of the individual customer.</param>
/// <param name="DisplayName">The computed display name for user presentation.</param>
/// <param name="Email">The primary electronic mail contact address.</param>
/// <param name="Phone">The primary telephone number.</param>
/// <param name="VatNumber">The value-added tax identifier, if registered.</param>
/// <param name="FiscalCode">The fiscal tax identification code.</param>
/// <param name="UidNumber">The Swiss enterprise identifier (UID), if applicable.</param>
/// <param name="SdiCode">The SDI recipient code for Italian electronic billing.</param>
/// <param name="Pec">The certified PEC electronic mail address for Italian billing.</param>
/// <param name="Iban">The bank account International Bank Account Number for payments.</param>
/// <param name="QrIban">The Swiss QR-IBAN account number for QR-bill generation.</param>
/// <param name="PaymentTermsDays">The standard credit payment terms in days.</param>
/// <param name="City">The municipality or city name of the customer billing address.</param>
/// <param name="AddressLine">The street name and house number of the customer billing address.</param>
public record CustomerDto(
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
    string? Pec,
    string? Iban,
    string? QrIban,
    int PaymentTermsDays,
    string? City,
    string? AddressLine);
