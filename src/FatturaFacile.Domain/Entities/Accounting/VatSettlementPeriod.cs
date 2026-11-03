// <copyright file="VatSettlementPeriod.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Accounting;

/// <summary>
/// Represents a periodic VAT settlement and liquidation aggregate (liquidazione periodica IVA / MWST-Abrechnung)
/// calculating output VAT, input deductible VAT, previous credits carried forward, and final net tax liability or refund credit.
/// </summary>
public sealed class VatSettlementPeriod : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{

    /// <summary>
    /// Initializes a new instance of the <see cref="VatSettlementPeriod"/> class.
    /// </summary>
    private VatSettlementPeriod()
    {
    }

    /// <summary>
    /// Gets or sets the unique tenant identifier for multi-tenant data partitioning.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the fiscal calendar year of the settlement period (e.g. 2026).
    /// </summary>
    public int Year { get; private set; }

    /// <summary>
    /// Gets the sequence index of the period: 1 through 12 for monthly liquidations, or 1 through 4 for quarterly filings.
    /// </summary>
    public int PeriodNumber { get; private set; }

    /// <summary>
    /// Gets the settlement frequency type (Monthly or Quarterly).
    /// </summary>
    public VatSettlementPeriodType PeriodType { get; private set; }

    /// <summary>
    /// Gets the governing tax authority jurisdiction (e.g. Italy for LIPE or Switzerland for ESTV).
    /// </summary>
    public Jurisdiction Jurisdiction { get; private set; }

    /// <summary>
    /// Gets the aggregate output sales VAT collected from customers on supplies during the period (IVA a debito / geschuldete MWST).
    /// </summary>
    public decimal TotalOutputVat { get; private set; }

    /// <summary>
    /// Gets the aggregate deductible input VAT paid on business purchases and supplier invoices during the period (IVA a credito / Vorsteuer).
    /// </summary>
    public decimal TotalInputVat { get; private set; }

    /// <summary>
    /// Gets the unused tax credit balance brought forward from the preceding settlement period or annual return (credito periodo precedente).
    /// </summary>
    public decimal PriorCreditCarriedForward { get; private set; }

    /// <summary>
    /// Gets the net tax amount owed to the revenue authority (debito d'imposta), computed as the maximum of zero and
    /// output VAT minus input VAT and prior credits.
    /// </summary>
    public decimal NetVatPayable => Math.Max(
                                             val1: 0m,
                                             val2: TotalOutputVat - TotalInputVat - PriorCreditCarriedForward);

    /// <summary>
    /// Gets the net deductible tax surplus resulting in a credit balance to carry forward to subsequent periods (credito residuo),
    /// computed as Max(0, TotalInputVat + PriorCreditCarriedForward - TotalOutputVat).
    /// </summary>
    public decimal NetVatCredit => Math.Max(
                                            val1: 0m,
                                            val2: TotalInputVat + PriorCreditCarriedForward - TotalOutputVat);

    /// <summary>
    /// Gets a value indicating whether this settlement period has been closed, locked, and finalized against further ledger adjustments.
    /// </summary>
    public bool IsFinalized { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when this settlement period was finalized and locked, or <see langword="null"/> if still open.
    /// </summary>
    public DateTime? FinalizedAtUtc { get; private set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this settlement record was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the user identifier or system process that created this settlement record.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this settlement record was last modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the user identifier or system process that last updated this settlement record.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates a new <see cref="VatSettlementPeriod"/> aggregate capturing tax figures for a given liquidation timeframe.
    /// </summary>
    /// <param name="data">The data transfer object containing the settlement period details.</param>
    /// <returns>A new instance of <see cref="VatSettlementPeriod"/> initialized with the specified parameters.</returns>
    public static VatSettlementPeriod Create(
        VatSettlementPeriodData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return new VatSettlementPeriod
        {
            Id                        = Guid.NewGuid(),
            TenantId                  = data.TenantId,
            Year                      = data.Year,
            PeriodNumber              = data.PeriodNumber,
            PeriodType                = data.PeriodType,
            Jurisdiction              = data.Jurisdiction,
            TotalOutputVat            = data.TotalOutputVat,
            TotalInputVat             = data.TotalInputVat,
            PriorCreditCarriedForward = data.PriorCreditCarriedForward,
            IsFinalized               = false,
            CreatedAtUtc              = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Finalizes the VAT settlement period, freezing tax computations, locking figures from modifications, and stamping the finalization timestamp.
    /// </summary>
    public void FinalizeSettlement()
    {
        IsFinalized    = true;
        FinalizedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc   = DateTime.UtcNow;
    }

}
