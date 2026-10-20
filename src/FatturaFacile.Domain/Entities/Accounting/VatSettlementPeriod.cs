using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Accounting;

/// <summary>
/// Represents vat settlement period.
/// </summary>
public sealed class VatSettlementPeriod : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the year.
    /// </summary>
    public int Year { get; private set; }

/// <summary>
    /// Gets the 1..12 for monthly or 1..4 for quarterly.
    /// </summary>
    public int PeriodNumber { get; private set; }

/// <summary>
    /// Gets the period type.
    /// </summary>
    public VatSettlementPeriodType PeriodType { get; private set; }

/// <summary>
    /// Gets the jurisdiction.
    /// </summary>
    public Jurisdiction Jurisdiction { get; private set; }

/// <summary>
    /// Gets the total output vat.
    /// </summary>
    public decimal TotalOutputVat { get; private set; }

/// <summary>
    /// Gets the total input vat.
    /// </summary>
    public decimal TotalInputVat { get; private set; }

/// <summary>
    /// Gets the prior credit carried forward.
    /// </summary>
    public decimal PriorCreditCarriedForward { get; private set; }

/// <summary>
    /// Gets the net vat payable.
    /// </summary>
    public decimal NetVatPayable => Math.Max(0m, TotalOutputVat - TotalInputVat - PriorCreditCarriedForward);

/// <summary>
    /// Gets the net vat credit.
    /// </summary>
    public decimal NetVatCredit => Math.Max(0m, TotalInputVat + PriorCreditCarriedForward - TotalOutputVat);

/// <summary>
    /// Gets a value indicating whether finalized.
    /// </summary>
    public bool IsFinalized { get; private set; }

/// <summary>
    /// Gets the finalized at utc.
    /// </summary>
    public DateTime? FinalizedAtUtc { get; private set; }

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

private VatSettlementPeriod()
    {
    }

/// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="year">The year.</param>
    /// <param name="periodNumber">The period number.</param>
    /// <param name="type">The type.</param>
    /// <param name="jurisdiction">The jurisdiction.</param>
    /// <param name="outputVat">The output vat.</param>
    /// <param name="inputVat">The input vat.</param>
    /// <param name="priorCredit">The prior credit.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static VatSettlementPeriod Create(
        Guid tenantId,
        int year,
        int periodNumber,
        VatSettlementPeriodType type,
        Jurisdiction jurisdiction,
        decimal outputVat,
        decimal inputVat,
        decimal priorCredit = 0m)
    {
        return new VatSettlementPeriod
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Year = year,
            PeriodNumber = periodNumber,
            PeriodType = type,
            Jurisdiction = jurisdiction,
            TotalOutputVat = outputVat,
            TotalInputVat = inputVat,
            PriorCreditCarriedForward = priorCredit,
            IsFinalized = false,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

/// <summary>
    /// Finalize Settlement.
    /// </summary>
    public void FinalizeSettlement()
    {
        IsFinalized = true;
        FinalizedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
