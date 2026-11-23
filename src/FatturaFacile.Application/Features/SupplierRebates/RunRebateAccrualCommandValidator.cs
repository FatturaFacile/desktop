// <copyright file="RunRebateAccrualCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using FluentValidation;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Validator for <see cref="RunRebateAccrualCommand"/>. The messages are localization keys.
/// </summary>
public sealed class RunRebateAccrualCommandValidator : AbstractValidator<RunRebateAccrualCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RunRebateAccrualCommandValidator"/> class.
    /// </summary>
    public RunRebateAccrualCommandValidator()
    {
        _ = RuleFor(x => x.AgreementId).NotEmpty().WithMessage(SupplierRebatesErrors.Required);
        _ = RuleFor(x => x.PeriodStart)
            .Must((command, start) => start is null || command.PeriodEnd is null || start <= command.PeriodEnd)
            .WithMessage(SupplierRebatesErrors.DatesOrder);
    }
}
