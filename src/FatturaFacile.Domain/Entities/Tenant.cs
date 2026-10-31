// <copyright file="Tenant.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents an isolated enterprise tenant organization, workspace, and legal billing entity.
/// </summary>
public class Tenant : EntityBase, ISoftDeletable
{
    /// <summary>
    /// Gets or sets the commercial trade name or display title of the tenant organization.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the formal legal registered corporate name.
    /// </summary>
    public string? LegalName { get; set; }

    /// <summary>
    /// Gets or sets the national fiscal code or tax identification number.
    /// </summary>
    public string? FiscalCode { get; set; }

    /// <summary>
    /// Gets or sets the value-added tax identifier (Partita IVA / UID MWST).
    /// </summary>
    public string? VatNumber { get; set; }

    /// <summary>
    /// Gets or sets the Swiss business unique identification number (UID / IDE).
    /// </summary>
    public string? UidNumber { get; set; }

    /// <summary>
    /// Gets or sets the primary country of fiscal residence (ISO 3166-1 alpha-2, e.g. "CH", "IT").
    /// </summary>
    public string Country { get; set; } = "CH";

    /// <summary>
    /// Gets or sets the default base accounting and invoicing currency (e.g. "CHF", "EUR").
    /// </summary>
    public string DefaultCurrency { get; set; } = "CHF";

    /// <summary>
    /// Gets or sets the default bank account IBAN for commercial incoming payments.
    /// </summary>
    public string? Iban { get; set; }

    /// <summary>
    /// Gets or sets the default Swiss QR-IBAN for Swiss QR-Bill remittances.
    /// </summary>
    public string? QrIban { get; set; }

    /// <summary>
    /// Gets or sets the electronic invoicing recipient routing code (Codice SDI / Destinatario).
    /// </summary>
    public string? SdiCode { get; set; }

    /// <summary>
    /// Gets or sets the certified email address for official legal and tax communications.
    /// </summary>
    public string? PecEmail { get; set; }

    /// <summary>
    /// Gets or sets serialized JSON containing tenant-specific settings and preferences.
    /// </summary>
    public string? SettingsJson { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this tenant workspace is under active maintenance lock.
    /// </summary>
    public bool IsMaintenance { get; set; }

    /// <inheritdoc />
    public bool IsDeleted { get; set; }

    /// <inheritdoc />
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the collection of user membership mappings associated with this tenant.
    /// </summary>
    public ICollection<TenantUser> TenantUsers { get; set; } = new List<TenantUser>();

    /// <summary>
    /// Gets or sets the collection of invoices issued by this tenant.
    /// </summary>
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    /// <summary>
    /// Gets or sets the collection of customers and suppliers managed by this tenant.
    /// </summary>
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();

    /// <summary>
    /// Gets or sets the collection of VAT rates and tax rules configured for this tenant.
    /// </summary>
    public ICollection<VatCode> VatCodes { get; set; } = new List<VatCode>();
}
