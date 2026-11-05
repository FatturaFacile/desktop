// <copyright file="InvoiceSdiMetadata.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Contains Italian SDI (Sistema di Interscambio) FatturaPA compliance metadata for an invoice.
/// </summary>
public class InvoiceSdiMetadata : IBaseEntity
{

    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the foreign key identifier of the linked invoice.
    /// </summary>
    public Guid InvoiceId { get; set; }

    /// <summary>
    /// Gets or sets the transmission format ("FPR12" for B2B/B2C private, "FPA12" for Public Administration).
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
    /// Gets or sets the generated XML file name (e.g., IT01234567890_00001.xml).
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Gets or sets the raw XML payload formatted according to FatturaPA specifications.
    /// </summary>
    public string? XmlContent { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the invoice was transmitted to the SDI channel.
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the delivery receipt was acknowledged by SDI.
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

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
