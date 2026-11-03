// <copyright file="ExecutiveKpiSummary.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Dashboard;

/// <summary>
/// Represents an executive-level performance and financial summary snapshot for a tenant,
/// tracking multi-currency revenue, open accounts receivable, and accrued VAT liabilities across Italian and Swiss fiscal regimes.
/// </summary>
public sealed class ExecutiveKpiSummary : Entity<Guid>, ITenantEntity
{

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutiveKpiSummary"/> class.
    /// </summary>
    private ExecutiveKpiSummary()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization owning this KPI snapshot.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the reporting period label or code (e.g., "2026-Q1", "2026-10", "YTD-2026").
    /// </summary>
    public string Period { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the cumulative invoiced sales revenue denominated in Euros (EUR) for the period.
    /// </summary>
    public Money TotalRevenueEur { get; private set; } = Money.Euro(amount: 0);

    /// <summary>
    /// Gets the cumulative invoiced sales revenue denominated in Swiss Francs (CHF) for the period.
    /// </summary>
    public Money TotalRevenueChf { get; private set; } = Money.Chf(amount: 0);

    /// <summary>
    /// Gets the total pending or overdue accounts receivable balance denominated in Euros (EUR).
    /// </summary>
    public Money PendingReceivablesEur { get; private set; } = Money.Euro(amount: 0);

    /// <summary>
    /// Gets the total pending or overdue accounts receivable balance denominated in Swiss Francs (CHF).
    /// </summary>
    public Money PendingReceivablesChf { get; private set; } = Money.Chf(amount: 0);

    /// <summary>
    /// Gets the estimated net VAT liability owed to the Italian tax agency (Agenzia delle Entrate / Erario) for the period.
    /// </summary>
    public Money PendingVatLiabilityEur { get; private set; } = Money.Euro(amount: 0);

    /// <summary>
    /// Gets the estimated net VAT/MWST liability owed to the Swiss Federal Tax Administration (ESTV / AFC) for the period.
    /// </summary>
    public Money PendingVatLiabilityChf { get; private set; } = Money.Chf(amount: 0);

    /// <summary>
    /// Gets the calendar date of the closest upcoming statutory fiscal, VAT, or social security payment deadline, or <see langword="null"/> if none is pending.
    /// </summary>
    public DateTime? NextTaxDeadline { get; private set; }

    /// <summary>
    /// Gets the primary operational fiscal jurisdiction (Italy or Switzerland) under which the tenant primarily reports.
    /// </summary>
    public Jurisdiction PrimaryJurisdiction { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when this executive KPI summary calculation was executed.
    /// </summary>
    public DateTime CalculatedAtUtc { get; private set; }

    /// <summary>
    /// Creates and initializes a new <see cref="ExecutiveKpiSummary"/> snapshot instance.
    /// </summary>
    /// <param name="data">The parameter object containing KPI values and figures.</param>
    /// <returns>A fully configured <see cref="ExecutiveKpiSummary"/> entity.</returns>
    public static ExecutiveKpiSummary Create(
        ExecutiveKpiSummaryData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return new ExecutiveKpiSummary
        {
            Id                     = Guid.NewGuid(),
            TenantId               = data.TenantId,
            Period                 = data.Period,
            TotalRevenueEur        = data.RevenueEur,
            TotalRevenueChf        = data.RevenueChf,
            PendingReceivablesEur  = data.ReceivablesEur,
            PendingReceivablesChf  = data.ReceivablesChf,
            PendingVatLiabilityEur = data.VatEur,
            PendingVatLiabilityChf = data.VatChf,
            NextTaxDeadline        = data.NextTaxDeadline,
            PrimaryJurisdiction    = data.Jurisdiction,
            CalculatedAtUtc        = DateTime.UtcNow,
        };
    }

}
