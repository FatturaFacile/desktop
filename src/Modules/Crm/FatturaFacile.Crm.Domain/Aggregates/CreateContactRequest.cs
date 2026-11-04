// <copyright file="CreateContactRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Crm.Domain.Aggregates;

/// <summary>
/// Represents a request to create a new contact within the CRM system.
/// </summary>
/// <param name="TenantId">The unique identifier of the tenant.</param>
/// <param name="Name">The name of the contact.</param>
/// <param name="ContactType">The type of the contact.</param>
/// <param name="CountryCode">The country code of the contact.</param>
/// <param name="VatNumber">The VAT number of the contact.</param>
/// <param name="TaxCode">The tax code of the contact.</param>
/// <param name="Email">The email address of the contact.</param>
/// <param name="SdiCode">The SDI code of the contact.</param>
public sealed record CreateContactRequest(
    Guid TenantId,
    string Name,
    ContactType ContactType,
    string CountryCode = "IT",
    string? VatNumber = null,
    string? TaxCode = null,
    string? Email = null,
    string? SdiCode = null);
