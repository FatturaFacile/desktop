// <copyright file="Contact.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents a commercial business entity or individual contact (client, customer, or vendor supplier).
/// </summary>
public class Contact : EntityBase, IMustHaveTenant, ISoftDeletable
{
    /// <inheritdoc />
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the commercial category of the contact (Customer or Supplier).
    /// </summary>
    public ContactType Type { get; set; } = ContactType.Customer;

    /// <summary>
    /// Gets or sets the two-letter ISO country code (e.g. "CH", "IT").
    /// </summary>
    public string Country { get; set; } = "CH";

    /// <summary>
    /// Gets or sets the legal registered company or business trade name.
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the personal first name for individual contacts or representatives.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the personal surname/family name for individual contacts.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the primary commercial email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the primary telephone or mobile contact number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the value-added tax identifier (Partita IVA / UID MWST).
    /// </summary>
    public string? VatNumber { get; set; }

    /// <summary>
    /// Gets or sets the national fiscal code or taxpayer identification number (Codice Fiscale).
    /// </summary>
    public string? FiscalCode { get; set; }

    /// <summary>
    /// Gets or sets the Swiss business unique identification number (UID / IDE).
    /// </summary>
    public string? UidNumber { get; set; }

    /// <summary>
    /// Gets or sets the Italian electronic invoicing recipient routing code (Codice SDI / Destinatario).
    /// </summary>
    public string? SdiCode { get; set; }

    /// <summary>
    /// Gets or sets the Italian certified electronic mail address (Posta Elettronica Certificata - PEC).
    /// </summary>
    public string? Pec { get; set; }

    /// <summary>
    /// Gets or sets the default International Bank Account Number (IBAN) for payments.
    /// </summary>
    public string? Iban { get; set; }

    /// <summary>
    /// Gets or sets the Swiss QR-IBAN for QR-bill slip generation.
    /// </summary>
    public string? QrIban { get; set; }

    /// <summary>
    /// Gets or sets the standard commercial payment maturity term in days (defaults to 30 days).
    /// </summary>
    public int PaymentTermsDays { get; set; } = 30;

    /// <summary>
    /// Gets or sets the street address and building number.
    /// </summary>
    public string? AddressLine { get; set; }

    /// <summary>
    /// Gets or sets the postal/ZIP code.
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the city or municipality name.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Gets or sets the administrative state, province code, or Swiss canton.
    /// </summary>
    public string? Province { get; set; }

    /// <inheritdoc />
    public bool IsDeleted { get; set; }

    /// <inheritdoc />
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the navigation property for the owning tenant workspace.
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Gets or sets the collection of invoices billed to this contact.
    /// </summary>
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    /// <summary>
    /// Gets the formatted display name prioritizing company name over personal individual names.
    /// </summary>
    public string DisplayName => !string.IsNullOrWhiteSpace(CompanyName)
        ? CompanyName
        : $"{FirstName} {LastName}".Trim();
}
