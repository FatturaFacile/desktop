// <copyright file="InvoiceSdiDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer object for Italian SDI FatturaPA metadata.
/// </summary>
public sealed class InvoiceSdiDto
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
    /// Gets or sets the transmission format ("FPR12" or "FPA12").
    /// </summary>
    public string TransmissionFormat { get; set; } = "FPR12";

    /// <summary>
    /// Gets or sets the unique transaction identifier assigned by the SDI portal.
    /// </summary>
    public string? SdiIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the SDI transmission state ("draft", "validated", "sent", "delivered", "rejected").
    /// </summary>
    public string SdiStatus { get; set; } = "draft";

    /// <summary>
    /// Gets or sets the XML file name.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Gets or sets the XML payload.
    /// </summary>
    public string? XmlContent { get; set; }

    /// <summary>
    /// Gets or sets the pre-transmission validation errors (one per line).
    /// </summary>
    public string? ValidationErrors { get; set; }

    /// <summary>
    /// Gets or sets the transmission timestamp.
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Gets or sets the delivery confirmation timestamp.
    /// </summary>
    public DateTime? DeliveredAt { get; set; }
}
