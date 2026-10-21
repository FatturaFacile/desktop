// <copyright file="TransportDocument.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Inventory;

/// <summary>
/// Represents transport document.
/// </summary>
public sealed class TransportDocument : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private TransportDocument()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the dDT-2026/001.
    /// </summary>
    public string DocumentNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the document date.
    /// </summary>
    public DateTime DocumentDate { get; private set; }

    /// <summary>
    /// Gets the client id.
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Gets the destination address.
    /// </summary>
    public Address DestinationAddress { get; private set; } = null!;

    /// <summary>
    /// Gets the vendita, Conto Visione, Riparazione.
    /// </summary>
    public string TransportationReason { get; private set; } = "Vendita";

    /// <summary>
    /// Gets the carrier name.
    /// </summary>
    public string CarrierName { get; private set; } = "Mittente";

    /// <summary>
    /// Gets the goods appearance.
    /// </summary>
    public string GoodsAppearance { get; } = "Cartoni";

    /// <summary>
    /// Gets the number of packages.
    /// </summary>
    public int NumberOfPackages { get; private set; } = 1;

    /// <summary>
    /// Gets the total weight kg.
    /// </summary>
    public decimal TotalWeightKg { get; private set; }

    /// <summary>
    /// Gets the cross-border customs notes IT-CH.
    /// </summary>
    public string? CustomsCmrNotes { get; private set; }

    /// <summary>
    /// Gets or sets the created at utc.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the created by.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the updated at utc.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the updated by.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="number">The number.</param>
    /// <param name="date">The date.</param>
    /// <param name="clientId">The client id.</param>
    /// <param name="destination">The destination.</param>
    /// <param name="reason">The reason.</param>
    /// <param name="carrier">The carrier.</param>
    /// <param name="packages">The packages.</param>
    /// <param name="weightKg">The weight kg.</param>
    /// <param name="customsCmr">The customs cmr.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
