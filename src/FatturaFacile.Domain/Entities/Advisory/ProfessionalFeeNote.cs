// <copyright file="ProfessionalFeeNote.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Advisory;

/// <summary>
/// Represents a professional fee note (avviso di parcella or fattura pro-forma) issued by a certified professional,
/// managing specialized legal and fiscal line items such as pension fund contributions (Cassa di Previdenza),
/// non-taxable out-of-pocket expenses (Spese anticipate ex art. 15), and withholding tax deductions (Ritenuta d'acconto).
/// </summary>
public sealed class ProfessionalFeeNote : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private ProfessionalFeeNote()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization issuing this fee note.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique document sequence number identifying the fee note (e.g., "PARC-2026/01").
    /// </summary>
    public string DocumentNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the unique identifier of the associated client matter or case file.
    /// </summary>
    public Guid ClientMatterId { get; private set; }

    /// <summary>
    /// Gets the accounting or issuance date of the fee note.
    /// </summary>
    public DateTime DocumentDate { get; private set; }

    /// <summary>
    /// Gets the billing currency denomination for fee totals and tax calculations.
    /// </summary>
    public Currency Currency { get; private set; } = Currency.EUR;

    /// <summary>
    /// Gets the total taxable amount of gross professional fees earned before pension surcharges and taxes.
    /// </summary>
    public decimal GrossProfessionalFees { get; private set; }

    /// <summary>
    /// Gets non-taxable client-reimbursed out-of-pocket expenses excluded from the VAT base (e.g., stamp duties, legal filings under Italian DPR 633/72 art. 15).
    /// </summary>
    public decimal ExpensesNonTaxable { get; private set; }

    /// <summary>
    /// Gets the statutory professional pension fund surcharge percentage (e.g., 4% for Cassa Forense or Dottori Commercialisti).
    /// </summary>
    public decimal ItCassaPrevidenzialePercentage { get; private set; } =
        4.0m; // 4% Cassa Forense/Dottori Commercialisti

    /// <summary>
    /// Gets the calculated monetary amount of the professional pension contribution (Cassa Previdenziale).
    /// </summary>
    public decimal ItCassaPrevidenzialeAmount { get; private set; }

    /// <summary>
    /// Gets the statutory withholding tax rate (Ritenuta d'acconto, typically 20% in Italy) deducted by corporate clients acting as withholding agents (sostituto d'imposta).
    /// </summary>
    public decimal ItRitenutaAccontoPercentage { get; private set; } = 20.0m;

    /// <summary>
    /// Gets the monetary amount withheld by the client as statutory advance income tax (Ritenuta d'acconto).
    /// </summary>
    public decimal ItRitenutaAccontoAmount { get; private set; }

    /// <summary>
    /// Gets the Swiss fiduciary withholding tax percentage applied to advisory retainers where statutory mandates require tax deduction at source.
    /// </summary>
    public decimal ChFiduciaryTaxPercentage { get; }

    /// <summary>
    /// Gets the calculated Swiss fiduciary withholding tax monetary deduction.
    /// </summary>
    public decimal ChFiduciaryTaxAmount { get; }

    /// <summary>
    /// Gets the total value-added tax calculated on taxable professional fees plus applicable pension contributions.
    /// </summary>
    public decimal TotalVat { get; private set; }

    /// <summary>
    /// Gets the net amount payable by the client, computed as gross fees plus pension surcharge, VAT, and non-taxable expenses, minus withholding tax.
    /// </summary>
    public decimal NetPayable { get; private set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this professional fee note was originally generated.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier or username of the user who generated this fee note.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this professional fee note was last updated.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier or username of the user who last updated this fee note.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates a professional fee note tailored for Italian fiscal regulations, applying statutory Cassa Previdenziale,
    /// Value Added Tax (IVA), and advance withholding tax (Ritenuta d'acconto).
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="docNumber">The unique reference document number.</param>
    /// <param name="matterId">The associated client matter identifier.</param>
    /// <param name="date">The issuance date of the fee note.</param>
    /// <param name="grossFees">The gross taxable professional fees.</param>
    /// <param name="nonTaxableExpenses">Out-of-pocket expenses exempt from VAT under Italian DPR 633/72 art. 15.</param>
    /// <param name="vatRate">The standard or applicable VAT rate percentage (defaults to 22.0%).</param>
    /// <param name="cassaPercent">The mandatory pension fund contribution percentage (defaults to 4.0%).</param>
    /// <param name="ritenutaPercent">The statutory withholding tax percentage (defaults to 20.0%).</param>
    /// <returns>A newly calculated and initialized <see cref="ProfessionalFeeNote"/> instance.</returns>
    public static ProfessionalFeeNote CreateItalian(
        Guid tenantId,
        string docNumber,
        Guid matterId,
        DateTime date,
        decimal grossFees,
        decimal nonTaxableExpenses,
        decimal vatRate = 22.0m,
        decimal cassaPercent = 4.0m,
        decimal ritenutaPercent = 20.0m)
    {
        var cassaAmt = decimal.Round(
            grossFees * (cassaPercent / 100m),
            2,
            MidpointRounding.AwayFromZero);
        var vatBase = grossFees + cassaAmt;
        var vatAmt = decimal.Round(
            vatBase * (vatRate / 100m),
            2,
            MidpointRounding.AwayFromZero);
        var ritenutaAmt = decimal.Round(
            grossFees * (ritenutaPercent / 100m),
            2,
            MidpointRounding.AwayFromZero);
        var netPayable = grossFees + cassaAmt + vatAmt + nonTaxableExpenses - ritenutaAmt;

        return new ProfessionalFeeNote
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DocumentNumber = docNumber?.Trim().ToUpperInvariant() ?? string.Empty,
            ClientMatterId = matterId,
            DocumentDate = date,
            Currency = Currency.EUR,
            GrossProfessionalFees = grossFees,
            ExpensesNonTaxable = nonTaxableExpenses,
            ItCassaPrevidenzialePercentage = cassaPercent,
            ItCassaPrevidenzialeAmount = cassaAmt,
            ItRitenutaAccontoPercentage = ritenutaPercent,
            ItRitenutaAccontoAmount = ritenutaAmt,
            TotalVat = vatAmt,
            NetPayable = netPayable,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
