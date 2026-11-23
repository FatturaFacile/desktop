// <copyright file="IntentLetter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A dichiarazione d'intento (lettera d'intento) of a habitual exporter: the protocol of the Agenzia delle Entrate receipt, the plafond and the period of validity. It is editable offline and synced with the web-api.
/// </summary>
public class IntentLetter : ITenantScopedEntity
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
    /// Gets or sets the customer (received) or supplier (issued) contact.
    /// </summary>
    public Guid ContactId { get; set; }

    /// <summary>
    /// Gets or sets whether the letter was received or issued.
    /// </summary>
    public IntentLetterDirection Direction { get; set; }

    /// <summary>
    /// Gets or sets the telematic protocol of the receipt of the tax authority.
    /// </summary>
    public string ProtocolNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date of the receipt.
    /// </summary>
    public DateTime ReceiptDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the year of the plafond.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the authorised plafond.
    /// </summary>
    public decimal PlafondAmount { get; set; }

    /// <summary>
    /// Gets or sets the erosion reported by the web-api at the last sync.
    /// </summary>
    public decimal ServerConsumedAmount { get; set; }

    /// <summary>
    /// Gets or sets the first day of validity.
    /// </summary>
    public DateTime ValidFrom { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the last day of validity.
    /// </summary>
    public DateTime ValidTo { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets free notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the sync state.
    /// </summary>
    public VatComplianceSyncState SyncState { get; set; } = VatComplianceSyncState.PendingCreate;

    /// <summary>
    /// Gets or sets the identifier of the letter on the web-api.
    /// </summary>
    public Guid? ServerId { get; set; }

    /// <summary>
    /// Gets or sets the reason the server refused the last change.
    /// </summary>
    public string? SyncError { get; set; }

    /// <summary>
    /// Gets or sets the logical deletion instant in UTC.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
