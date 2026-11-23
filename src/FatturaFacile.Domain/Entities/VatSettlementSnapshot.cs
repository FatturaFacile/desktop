// <copyright file="VatSettlementSnapshot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A VAT settlement (liquidazione periodica) of the web-api kept offline, to reconcile the periods with the local computation and the Erario c/IVA account.
/// </summary>
public class VatSettlementSnapshot : ITenantScopedEntity
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
    /// Gets or sets the identifier of the settlement on the web-api.
    /// </summary>
    public Guid ServerId { get; set; }

    /// <summary>
    /// Gets or sets the periodicity: monthly or quarterly.
    /// </summary>
    public string PeriodType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the year.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the month or quarter.
    /// </summary>
    public int Period { get; set; }

    /// <summary>
    /// Gets or sets the first day.
    /// </summary>
    public DateTime PeriodFrom { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the last day.
    /// </summary>
    public DateTime PeriodTo { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the VAT on sales.
    /// </summary>
    public decimal OutputVat { get; set; }

    /// <summary>
    /// Gets or sets the deductible VAT on purchases.
    /// </summary>
    public decimal InputVat { get; set; }

    /// <summary>
    /// Gets or sets the credit carried in.
    /// </summary>
    public decimal PreviousCredit { get; set; }

    /// <summary>
    /// Gets or sets the debit carried in.
    /// </summary>
    public decimal PreviousDebit { get; set; }

    /// <summary>
    /// Gets or sets the interest of the quarterly liquidation.
    /// </summary>
    public decimal Interest { get; set; }

    /// <summary>
    /// Gets or sets the amount to pay (negative is a credit).
    /// </summary>
    public decimal PayableAmount { get; set; }

    /// <summary>
    /// Gets or sets the status: calculated, posted or paid.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the journal entry that posted the settlement.
    /// </summary>
    public Guid? SettlementJournalId { get; set; }

    /// <summary>
    /// Gets or sets when the settlement was pulled.
    /// </summary>
    public DateTime SyncedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
