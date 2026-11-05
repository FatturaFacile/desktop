// <copyright file="InvoiceQrBillDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer object for Swiss QR-Bill payment information.
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
    /// Gets or sets the Swiss QR-IBAN.
    /// </summary>
    public string QrIban { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the 27-digit SCOR reference number.
    /// </summary>
    public string ScorReference { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creditor address information.
    /// </summary>
    public string CreditorInfo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the debtor address information.
    /// </summary>
    public string DebtorInfo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Swiss QR-Bill SVG or payload.
    /// </summary>
    public string? QrBillSvg { get; set; }
}
