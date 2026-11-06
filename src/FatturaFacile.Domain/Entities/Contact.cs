// <copyright file="Contact.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents a customer, vendor, or business contact within the ERP system.
/// </summary>
public class Contact : IOptionalTenantEntity
{

    /// <summary>
    /// Gets or sets the primary key identifier for the contact.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the optional owning tenant identifier. If null, the contact is global.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the relationship type ("client", "supplier", "both").
    /// </summary>
    public string Type { get; set; } = "client";

    /// <summary>
    /// Gets or sets the contact's country code ("CH", "IT", etc.).
    /// </summary>
    public string Country { get; set; } = "IT";

    /// <summary>
    /// Gets or sets the company or commercial name.
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Gets or sets the contact person first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the contact person last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the primary contact email address.
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
    /// Gets or sets the Italian fiscal code (Codice Fiscale).
    /// </summary>
    public string? FiscalCode { get; set; }

    /// <summary>
    /// Gets or sets the Swiss business identification number (UID).
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
    /// Gets or sets the standard bank account IBAN.
    /// </summary>
    public string? Iban { get; set; }

    /// <summary>
    /// Gets or sets the Swiss QR-IBAN for QR-bill payment generation.
    /// </summary>
    public string? QrIban { get; set; }

    /// <summary>
    /// Gets or sets standard payment terms in days (default 30).
    /// </summary>
    public int PaymentTermsDays { get; set; } = 30;

    /// <summary>
    /// Gets or sets the street name (structured address, required by QR-bill and FatturaPA).
    /// </summary>
    public string? Street { get; set; }

    /// <summary>
    /// Gets or sets the building / house number.
    /// </summary>
    public string? HouseNumber { get; set; }

    /// <summary>
    /// Gets or sets the postal code (CAP / NPA).
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the town / municipality.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Gets or sets the Italian province code (sigla, 2 letters) or Swiss canton code.
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the customer is an Italian public administration (FatturaPA FPA12 format).
    /// </summary>
    public bool IsPublicAdministration { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the record was created.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the record was last modified.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the soft delete timestamp.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets the full display name combining company name or personal name.
    /// </summary>
    public string DisplayName => !string.IsNullOrWhiteSpace(CompanyName)
        ? CompanyName
        : $"{FirstName} {LastName}".Trim();
}
