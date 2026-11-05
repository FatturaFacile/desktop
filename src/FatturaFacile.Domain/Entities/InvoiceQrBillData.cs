// <copyright file="InvoiceQrBillData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Contains Swiss QR-Bill (ISO 20022 compliant) payment part metadata for an invoice.
/// </summary>
public class InvoiceQrBillData : IBaseEntity
{

    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the foreign key identifier of the parent invoice.
    /// </summary>
    public Guid InvoiceId { get; set; }

    /// <summary>
    /// Gets or sets the Swiss QR-IBAN formatted according to SIX Interbank Clearing standards.
    /// </summary>
    public string QrIban { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the 27-digit SCOR structured reference number with Modulo 10 recursive check digit.
    /// </summary>
    public string ScorReference { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets structured creditor information (name, street, building, postal code, city, country).
    /// </summary>
    public string CreditorInfo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets structured debtor information (customer name and address).
    /// </summary>
    public string DebtorInfo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rendered SVG payload or Swiss QR code string representation.
    /// </summary>
    public string? QrBillSvg { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the navigation reference to the parent invoice.
    /// </summary>
    public Invoice? Invoice { get; set; }
}
