// <copyright file="InvoiceQrBillDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer object for Swiss QR-bill payment information.
/// </summary>
public sealed class InvoiceQrBillDto
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the invoice foreign key identifier.
    /// </summary>
    public Guid InvoiceId { get; set; }

    /// <summary>
    /// Gets or sets the creditor account (IBAN or QR-IBAN).
    /// </summary>
    public string QrIban { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reference type ("QRR", "SCOR" or "NON").
    /// </summary>
    public string ReferenceType { get; set; } = "NON";

    /// <summary>
    /// Gets or sets the payment reference (QRR or SCOR).
    /// </summary>
    public string PaymentReference { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creditor address information.
    /// </summary>
    public string CreditorInfo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the debtor address information.
    /// </summary>
    public string DebtorInfo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw Swiss QR code payload.
    /// </summary>
    public string? QrPayload { get; set; }

    /// <summary>
    /// Gets or sets the rendered payment part SVG.
    /// </summary>
    public string? QrBillSvg { get; set; }

    /// <summary>
    /// Gets or sets the validation errors (one per line).
    /// </summary>
    public string? ValidationErrors { get; set; }
}
