// <copyright file="CalculateInputTaxAdjustmentCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SwissVat;
using MediatR;

namespace FatturaFacile.Application.Features.SwissVat;

/// <summary>
/// Handles <see cref="CalculateInputTaxAdjustmentCommand"/>.
/// </summary>
public sealed class CalculateInputTaxAdjustmentCommandHandler
    : IRequestHandler<CalculateInputTaxAdjustmentCommand, InputTaxAdjustmentDto>
{
    /// <inheritdoc/>
    public Task<InputTaxAdjustmentDto> Handle(
        CalculateInputTaxAdjustmentCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var amount = InputTaxAdjustmentCalculator.Compute(
            request.AdjustmentType,
            request.OriginalCostChf,
            request.OriginalVatRate,
            request.YearsElapsed,
            request.IsRealEstate,
            request.RatioPercent);

        var dto = new InputTaxAdjustmentDto(
            Id: Guid.NewGuid(),
            AdjustmentType: request.AdjustmentType,
            AfcBox: InputTaxAdjustmentCalculator.TargetAfcBox(request.AdjustmentType),
            EffectiveDate: request.EffectiveDate,
            Description: request.Description,
            OriginalCostChf: request.OriginalCostChf,
            OriginalVatRate: request.OriginalVatRate,
            YearsElapsed: request.YearsElapsed,
            PrivateOrExemptRatio: request.RatioPercent,
            AdjustmentAmountChf: amount,
            LegalBasis: InputTaxAdjustmentCalculator.LegalBasis(request.AdjustmentType),
            Status: "draft");

        return Task.FromResult(dto);
    }
}
