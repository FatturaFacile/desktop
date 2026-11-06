// <copyright file="InvoiceQrBillData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Contains the Swiss QR-bill payment part data (Swiss Implementation Guidelines QR-bill, SPC version 0200) of an invoice.
/// </summary>
public class InvoiceQrBillData : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the foreign key identifier of the parent invoice.
    /// </summary>
    public Guid InvoiceId { get; set; }

    /// <summary>
    /// Gets or sets the creditor account (IBAN or QR-IBAN) printed on the payment part.
    /// </summary>
    public string QrIban { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reference type: "QRR" (QR reference, QR-IBAN only), "SCOR" (ISO 11649 creditor reference) or "NON".
    /// </summary>
    public string ReferenceType { get; set; } = "NON";

    /// <summary>
    /// Gets or sets the payment reference (27-digit QRR or ISO 11649 RF creditor reference), empty when the type is "NON".
    /// </summary>
    public string PaymentReference { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the structured creditor address, human readable.
    /// </summary>
    public string CreditorInfo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the structured debtor address, human readable.
    /// </summary>
    public string DebtorInfo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw Swiss QR code payload (SPC 0200, CR+LF separated) encoded in the QR symbol.
    /// </summary>
    public string? QrPayload { get; set; }

    /// <summary>
    /// Gets or sets the rendered payment part SVG including the real QR symbol and Swiss cross.
    /// </summary>
    public string? QrBillSvg { get; set; }

    /// <summary>
    /// Gets or sets the validation errors (one per line). When not empty, the QR-bill must not be issued.
    /// </summary>
    public string? ValidationErrors { get; set; }

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
