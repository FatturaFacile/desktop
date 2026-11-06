// <copyright file="InvoiceSdiMetadata.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Contains the Italian FatturaPA electronic invoice and its Sistema di Interscambio (SDI) transmission state.
/// </summary>
public class InvoiceSdiMetadata : ITenantScopedEntity
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
    /// Gets or sets the transmission format ("FPR12" for B2B/B2C, "FPA12" for public administrations).
    /// </summary>
    public string TransmissionFormat { get; set; } = "FPR12";

    /// <summary>
    /// Gets or sets the SDI file identifier (IdentificativoSdI) assigned by the SDI upon receipt. Null until transmitted.
    /// </summary>
    public string? SdiIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the lifecycle status: "generated", "invalid", "sent", "delivered" (RC), "rejected" (NS), "not_delivered" (MC).
    /// </summary>
    public string SdiStatus { get; set; } = "draft";

    /// <summary>
    /// Gets or sets the SDI file name (IT + IdCodice trasmittente + "_" + progressivo + ".xml").
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Gets or sets the generated FatturaPA XML content.
    /// </summary>
    public string? XmlContent { get; set; }

    /// <summary>
    /// Gets or sets the pre-transmission validation errors (one per line). When not empty the file must not be sent.
    /// </summary>
    public string? ValidationErrors { get; set; }

    /// <summary>
    /// Gets or sets the timestamp the file was handed over to the SDI channel.
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the SDI delivery receipt (Ricevuta di Consegna).
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
