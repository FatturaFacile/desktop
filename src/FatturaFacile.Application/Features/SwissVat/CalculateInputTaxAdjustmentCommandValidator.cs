// <copyright file="CalculateInputTaxAdjustmentCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.SwissVat;

/// <summary>
/// Validates <see cref="CalculateInputTaxAdjustmentCommand"/>.
/// </summary>
public sealed class CalculateInputTaxAdjustmentCommandValidator : AbstractValidator<CalculateInputTaxAdjustmentCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalculateInputTaxAdjustmentCommandValidator"/> class.
    /// </summary>
    public CalculateInputTaxAdjustmentCommandValidator()
    {
        _ = RuleFor(x => x.Description).NotEmpty();
        _ = RuleFor(x => x.OriginalCostChf).GreaterThan(0m);
        _ = RuleFor(x => x.OriginalVatRate).GreaterThan(0m);
        _ = RuleFor(x => x.YearsElapsed).GreaterThanOrEqualTo(0);
        _ = RuleFor(x => x.RatioPercent).InclusiveBetween(0m, 100m);
    }
}
