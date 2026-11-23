// <copyright file="CalculateSwissVatReturnCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;
using FluentValidation;

namespace FatturaFacile.Application.Features.SwissVat;

/// <summary>
/// Validates <see cref="CalculateSwissVatReturnCommand"/> before calculation.
/// </summary>
public sealed class CalculateSwissVatReturnCommandValidator : AbstractValidator<CalculateSwissVatReturnCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalculateSwissVatReturnCommandValidator"/> class.
    /// </summary>
    public CalculateSwissVatReturnCommandValidator()
    {
        _ = RuleFor(x => x.Input).NotNull();
        _ = RuleFor(x => x.Input.PeriodTo)
            .GreaterThanOrEqualTo(x => x.Input.PeriodFrom)
            .When(x => x.Input is not null);
        _ = RuleFor(x => x.Input.SssRatePrimary)
            .GreaterThan(0m)
            .When(x => x.Input is not null && x.Input.Method == SwissVatMethod.Saldosteuersatz);
    }
}
