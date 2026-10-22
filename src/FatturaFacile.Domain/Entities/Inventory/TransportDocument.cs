// <copyright file="TransportDocument.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Inventory;

/// <summary>
/// Represents a legal shipping delivery note or transport document (DDT - Documento di Trasporto under Italian DPR 472/96,
/// or Swiss Lieferschein / CMR consignment note), establishing fiscal proof of custody transfer, transportation reason,
/// package count, and cross-border customs declarations.
/// </summary>
public sealed class TransportDocument : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private TransportDocument()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant shipping company or consignor.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique progressive transport document identifier (e.g., "DDT-2026/001").
    /// </summary>
    public string DocumentNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the calendar date on which the goods leave the shipping facility.
    /// </summary>
    public DateTime DocumentDate { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the customer or recipient entity (destinatario).
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Gets the destination delivery address where goods are physically unloaded.
    /// </summary>
    public Address DestinationAddress { get; private set; } = null!;

    /// <summary>
    /// Gets the statutory legal reason for merchandise movement (Causale del trasporto, e.g., "Vendita", "Conto Visione", "Conto Lavorazione", "Riparazione").
    /// </summary>
    public string TransportationReason { get; private set; } = "Vendita";

    /// <summary>
    /// Gets the party responsible for physical freight haulage (Vettore / Porto, e.g., "Mittente", "Destinatario", or third-party courier name).
    /// </summary>
    public string CarrierName { get; private set; } = "Mittente";

    /// <summary>
    /// Gets the external visual packaging description (Aspetto esteriore dei beni, e.g., "Cartoni", "Pallet", "Sfuso").
    /// </summary>
    public string GoodsAppearance { get; } = "Cartoni";

    /// <summary>
    /// Gets the total number of parcels, boxes, or collis comprising the shipment.
    /// </summary>
    public int NumberOfPackages { get; private set; } = 1;

    /// <summary>
    /// Gets the gross weight of the freight shipment expressed in kilograms.
    /// </summary>
    public decimal TotalWeightKg { get; private set; }

    /// <summary>
    /// Gets cross-border customs endorsements, Carnet TIR, or international CMR consignment notations (e.g., for Swiss-Italian cross-border freight).
    /// </summary>
    public string? CustomsCmrNotes { get; private set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the transport document was issued.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier or username of the user who compiled this delivery document.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this transport document was last modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier or username of the user who last modified this document.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates a new delivery transport document (DDT) compliant with commercial and fiscal freight regulations.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="number">The progressive delivery note number (e.g., "DDT-2026/001").</param>
    /// <param name="date">The dispatch date of the goods.</param>
    /// <param name="clientId">The consignee client identifier.</param>
    /// <param name="destination">The delivery destination address.</param>
    /// <param name="reason">The statutory transportation reason (Causale di trasporto).</param>
    /// <param name="carrier">The freight carrier name or conveyance designation.</param>
    /// <param name="packages">The number of parcels or collis.</param>
    /// <param name="weightKg">The gross cargo weight in kilograms.</param>
    /// <param name="customsCmr">Optional cross-border customs declarations or CMR transport remarks.</param>
    /// <returns>A newly created <see cref="TransportDocument"/> aggregate instance.</returns>
    public static TransportDocument Create(
        Guid tenantId,
        string number,
        DateTime date,
        Guid clientId,
        Address destination,
        string reason,
        string carrier,
        int packages,
        decimal weightKg,
        string? customsCmr = null)
    {
        return new TransportDocument
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DocumentNumber = number,
            DocumentDate = date,
            ClientId = clientId,
            DestinationAddress = destination,
            TransportationReason = reason,
            CarrierName = carrier,
            NumberOfPackages = packages,
            TotalWeightKg = weightKg,
            CustomsCmrNotes = customsCmr,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
