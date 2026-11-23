// <copyright file="CalculateInputTaxAdjustmentCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.SwissVat;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.SwissVat;

/// <summary>
/// CQRS command to compute a LIVA Art. 30-33 input tax adjustment (Autoconsumo / Degrassazione / Mixed-use).
/// </summary>
/// <param name="AdjustmentType">Adjustment type.</param>
/// <param name="EffectiveDate">Effective date.</param>
/// <param name="Description">Description.</param>
/// <param name="OriginalCostChf">Original cost or base amount in CHF.</param>
/// <param name="OriginalVatRate">Original VAT rate percentage.</param>
/// <param name="YearsElapsed">Elapsed years since acquisition.</param>
/// <param name="IsRealEstate">True for real estate (20-year horizon).</param>
/// <param name="RatioPercent">Private or exempt usage ratio (0-100).</param>
public sealed record CalculateInputTaxAdjustmentCommand(
    InputTaxAdjustmentType AdjustmentType,
    DateOnly EffectiveDate,
    string Description,
    decimal OriginalCostChf,
    decimal OriginalVatRate,
    int YearsElapsed,
    bool IsRealEstate,
    decimal RatioPercent = 100m)
    : IRequest<InputTaxAdjustmentDto>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
