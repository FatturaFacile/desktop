// <copyright file="ReverseRebateAccrualRunCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using FluentValidation;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Validator for <see cref="ReverseRebateAccrualRunCommand"/>. The messages are localization keys.
/// </summary>
public sealed class ReverseRebateAccrualRunCommandValidator : AbstractValidator<ReverseRebateAccrualRunCommand>
{
    private const int ReasonLength = 200;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReverseRebateAccrualRunCommandValidator"/> class.
    /// </summary>
    public ReverseRebateAccrualRunCommandValidator()
    {
        _ = RuleFor(x => x.AgreementId).NotEmpty().WithMessage(SupplierRebatesErrors.Required);
        _ = RuleFor(x => x.RunId).NotEmpty().WithMessage(SupplierRebatesErrors.Required);
        _ = RuleFor(x => x.Reason).NotEmpty().WithMessage(SupplierRebatesErrors.Reason).MaximumLength(ReasonLength).WithMessage(SupplierRebatesErrors.Reason);
    }
}
