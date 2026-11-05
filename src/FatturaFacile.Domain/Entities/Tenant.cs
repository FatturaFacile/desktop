// <copyright file="Tenant.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents an enterprise company or organizational tenant within the ERP monolith.
/// </summary>
public class Tenant : IBaseEntity
{

    /// <summary>
    /// Gets or sets the primary key identifier for the tenant.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the trade name of the tenant.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the official registered legal name of the entity.
    /// </summary>
    public string LegalName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Italian fiscal code (Codice Fiscale) if applicable.
    /// </summary>
    public string? FiscalCode { get; set; }

    /// <summary>
    /// Gets or sets the VAT registration number (Partita IVA).
    /// </summary>
    public string? VatNumber { get; set; }

    /// <summary>
    /// Gets or sets the Swiss Business Identification Number (UID: CHE-xxx.xxx.xxx).
    /// </summary>
    public string? UidNumber { get; set; }

    /// <summary>
    /// Gets or sets the ISO country jurisdiction ("CH" for Switzerland, "IT" for Italy).
    /// </summary>
    public string Country { get; set; } = "IT";

    /// <summary>
    /// Gets or sets the default base currency ("CHF", "EUR").
    /// </summary>
    public string DefaultCurrency { get; set; } = "EUR";

    /// <summary>
    /// Gets or sets the company standard bank account IBAN.
    /// </summary>
    public string? Iban { get; set; }

    /// <summary>
    /// Gets or sets the Swiss QR-IBAN for ISO 20022 QR-bill collections.
    /// </summary>
    public string? QrIban { get; set; }

    /// <summary>
    /// Gets or sets the Italian SDI recipient code (Codice Destinatario, e.g. "0000000" or 7-character code).
    /// </summary>
    public string? SdiCode { get; set; }

    /// <summary>
    /// Gets or sets the certified email address (PEC - Posta Elettronica Certificata).
    /// </summary>
    public string? PecEmail { get; set; }

    /// <summary>
    /// Gets or sets optional JSON settings metadata.
    /// </summary>
    public string? Settings { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the tenant is in maintenance mode.
    /// </summary>
    public bool IsMaintenance { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the record was created.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the record was last modified.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the soft delete timestamp if deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
