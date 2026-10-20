using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Advisory;

/// <summary>
/// Represents professional fee note.
/// </summary>
public sealed class ProfessionalFeeNote : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the pARC-2026/01.
    /// </summary>
    public string DocumentNumber { get; private set; } = string.Empty;

/// <summary>
    /// Gets the client matter id.
    /// </summary>
    public Guid ClientMatterId { get; private set; }

/// <summary>
    /// Gets the document date.
    /// </summary>
    public DateTime DocumentDate { get; private set; }

/// <summary>
    /// Gets the currency.
    /// </summary>
    public Currency Currency { get; private set; } = Currency.EUR;

/// <summary>
    /// Gets the fee calculations.
    /// </summary>
    public decimal GrossProfessionalFees { get; private set; }

/// <summary>
    /// Gets the expenses non taxable.
    /// </summary>
    public decimal ExpensesNonTaxable { get; private set; }

/// <summary>
    /// Gets the italian specific statutory contributions.
    /// </summary>
    public decimal ItCassaPrevidenzialePercentage { get; private set; } = 4.0m; // 4% Cassa Forense/Dottori Commercialisti

/// <summary>
    /// Gets the it cassa previdenziale amount.
    /// </summary>
    public decimal ItCassaPrevidenzialeAmount { get; private set; }

/// <summary>
    /// Gets the 20% Ritenuta d&amp;#39;acconto.
    /// </summary>
    public decimal ItRitenutaAccontoPercentage { get; private set; } = 20.0m;

/// <summary>
    /// Gets the it ritenuta acconto amount.
    /// </summary>
    public decimal ItRitenutaAccontoAmount { get; private set; }

/// <summary>
    /// Gets the swiss specific fiduciary withholding.
    /// </summary>
    public decimal ChFiduciaryTaxPercentage { get; }

/// <summary>
    /// Gets the ch fiduciary tax amount.
    /// </summary>
    public decimal ChFiduciaryTaxAmount { get; }

/// <summary>
    /// Gets the total vat.
    /// </summary>
    public decimal TotalVat { get; private set; }

/// <summary>
    /// Gets the net payable.
    /// </summary>
    public decimal NetPayable { get; private set; }

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

private ProfessionalFeeNote()
    {
    }

/// <summary>
    /// Create Italian.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="docNumber">The doc number.</param>
    /// <param name="matterId">The matter id.</param>
    /// <param name="date">The date.</param>
    /// <param name="grossFees">The gross fees.</param>
    /// <param name="nonTaxableExpenses">The non taxable expenses.</param>
    /// <param name="vatRate">The vat rate.</param>
    /// <param name="cassaPercent">The cassa percent.</param>
    /// <param name="ritenutaPercent">The ritenuta percent.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
        var cassaAmt = decimal.Round(grossFees * (cassaPercent / 100m), 2, MidpointRounding.AwayFromZero);
        var vatBase = grossFees + cassaAmt;
        var vatAmt = decimal.Round(vatBase * (vatRate / 100m), 2, MidpointRounding.AwayFromZero);
        var ritenutaAmt = decimal.Round(grossFees * (ritenutaPercent / 100m), 2, MidpointRounding.AwayFromZero);
        var netPayable = grossFees + cassaAmt + vatAmt + nonTaxableExpenses - ritenutaAmt;

        return new ProfessionalFeeNote
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DocumentNumber = docNumber.Trim().ToUpperInvariant(),
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
