// <copyright file="InputTaxAdjustmentDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Data transfer object for a LIVA Art. 30-33 input tax adjustment.
/// </summary>
/// <param name="Id">Adjustment identifier.</param>
/// <param name="AdjustmentType">Adjustment type.</param>
/// <param name="AfcBox">Target AFC box (410, 415, or 420).</param>
/// <param name="EffectiveDate">Effective date.</param>
/// <param name="Description">Description.</param>
/// <param name="OriginalCostChf">Original asset cost in CHF.</param>
/// <param name="OriginalVatRate">Original VAT rate percentage.</param>
/// <param name="YearsElapsed">Elapsed depreciation years.</param>
/// <param name="PrivateOrExemptRatio">Private or exempt ratio percentage.</param>
/// <param name="AdjustmentAmountChf">Computed adjustment amount in CHF.</param>
/// <param name="LegalBasis">Statutory legal basis citation.</param>
/// <param name="Status">Approval status.</param>
public sealed record InputTaxAdjustmentDto(
    Guid Id,
    InputTaxAdjustmentType AdjustmentType,
    string AfcBox,
    DateOnly EffectiveDate,
    string Description,
    decimal? OriginalCostChf,
    decimal? OriginalVatRate,
    int? YearsElapsed,
    decimal? PrivateOrExemptRatio,
    decimal AdjustmentAmountChf,
    string LegalBasis,
    string Status);
