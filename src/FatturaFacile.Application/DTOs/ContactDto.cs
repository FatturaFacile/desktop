// <copyright file="ContactDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer object for a customer or vendor contact.
/// </summary>
public sealed class ContactDto
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the optional owning tenant identifier.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the contact type ("client", "supplier", "both").
    /// </summary>
    public string Type { get; set; } = "client";

    /// <summary>
    /// Gets or sets the country code.
    /// </summary>
    public string Country { get; set; } = "IT";

    /// <summary>
    /// Gets or sets the company or commercial trade name.
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Gets or sets the contact first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the contact last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the primary email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the VAT registration number.
    /// </summary>
    public string? VatNumber { get; set; }

    /// <summary>
    /// Gets or sets the Italian fiscal code.
    /// </summary>
    public string? FiscalCode { get; set; }

    /// <summary>
    /// Gets or sets the Swiss UID number.
    /// </summary>
    public string? UidNumber { get; set; }

    /// <summary>
    /// Gets or sets the Italian SDI recipient code.
    /// </summary>
    public string? SdiCode { get; set; }

    /// <summary>
    /// Gets or sets the certified email address (PEC).
    /// </summary>
    public string? Pec { get; set; }

    /// <summary>
    /// Gets or sets the bank account IBAN.
    /// </summary>
    public string? Iban { get; set; }

    /// <summary>
    /// Gets or sets the Swiss QR-IBAN.
    /// </summary>
    public string? QrIban { get; set; }

    /// <summary>
    /// Gets or sets the payment term days.
    /// </summary>
    public int PaymentTermsDays { get; set; } = 30;

    /// <summary>
    /// Gets or sets the street name.
    /// </summary>
    public string? Street { get; set; }

    /// <summary>
    /// Gets or sets the building / house number.
    /// </summary>
    public string? HouseNumber { get; set; }

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the town / municipality.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Gets or sets the province (IT) or canton (CH) code.
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the customer is an Italian public administration.
    /// </summary>
    public bool IsPublicAdministration { get; set; }

    /// <summary>
    /// Gets or sets the formatted display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
}
