// <copyright file="SwissVatInputTaxAdjustment.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Input tax adjustment under LIVA Art. 30-33 (Autoconsumo, Degrassazione, Mixed-use, Subsidy reduction).
/// </summary>
public class SwissVatInputTaxAdjustment : ITenantScopedEntity
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
    /// Gets or sets the adjustment type.
    /// </summary>
    public InputTaxAdjustmentType AdjustmentType { get; set; }

    /// <summary>
    /// Gets or sets the target AFC box (410, 415, or 420).
    /// </summary>
    public string AfcBox { get; set; } = "415";

    /// <summary>
    /// Gets or sets the effective date of the adjustment.
    /// </summary>
    public DateOnly EffectiveDate { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original asset acquisition cost in CHF when computed by residual value.
    /// </summary>
    public decimal? OriginalCostChf { get; set; }

    /// <summary>
    /// Gets or sets the original VAT rate percentage.
    /// </summary>
    public decimal? OriginalVatRate { get; set; }

    /// <summary>
    /// Gets or sets the elapsed depreciation years.
    /// </summary>
    public int? YearsElapsed { get; set; }

    /// <summary>
    /// Gets or sets the private or exempt usage ratio percentage.
    /// </summary>
    public decimal? PrivateOrExemptRatio { get; set; }

    /// <summary>
    /// Gets or sets the computed adjustment amount in CHF.
    /// </summary>
    public decimal AdjustmentAmountChf { get; set; }

    /// <summary>
    /// Gets or sets the statutory legal basis (e.g., Art. 31 LIVA).
    /// </summary>
    public string LegalBasis { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the workflow status (draft, approved, applied).
    /// </summary>
    public string Status { get; set; } = "draft";

    /// <summary>
    /// Gets or sets the UTC creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC last-updated timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
