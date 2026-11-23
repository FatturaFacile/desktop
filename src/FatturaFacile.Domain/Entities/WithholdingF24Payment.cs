// <copyright file="WithholdingF24Payment.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// The Modello F24 payment of the withholdings of one month: due date, payment tracking and sync with the web-api.
/// </summary>
public class WithholdingF24Payment : ITenantScopedEntity
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
    /// Gets or sets the reference year.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the reference month.
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// Gets or sets the F24 tax code.
    /// </summary>
    public string TaxCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the amount to pay.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the due date (the 16th of the following month).
    /// </summary>
    public DateTime DueDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the date of the payment.
    /// </summary>
    public DateTime? PaidDate { get; set; }

    /// <summary>
    /// Gets or sets the reference of the payment (F24 receipt, bank movement).
    /// </summary>
    public string? PaymentReference { get; set; }

    /// <summary>
    /// Gets or sets the withholding settlement of the web-api.
    /// </summary>
    public Guid? SettlementServerId { get; set; }

    /// <summary>
    /// Gets or sets the sync state of the payment.
    /// </summary>
    public VatComplianceSyncState SyncState { get; set; } = VatComplianceSyncState.Synced;

    /// <summary>
    /// Gets or sets the reason the server refused the payment.
    /// </summary>
    public string? SyncError { get; set; }

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
