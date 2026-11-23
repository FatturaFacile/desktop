// <copyright file="SaveRebateContributionCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using FluentValidation;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Validator for <see cref="SaveRebateContributionCommand"/>. The messages are localization keys.
/// </summary>
public sealed class SaveRebateContributionCommandValidator : AbstractValidator<SaveRebateContributionCommand>
{
    private const int DescriptionLength = 200;
    private const decimal MaxAmount = 9999999999999m;

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveRebateContributionCommandValidator"/> class.
    /// </summary>
    public SaveRebateContributionCommandValidator()
    {
        _ = RuleFor(x => x.AgreementId).NotEmpty().WithMessage(SupplierRebatesErrors.Required);
        _ = RuleFor(x => x.Input.Kind)
            .Must(kind => kind is RebateCodes.KindMarketing or RebateCodes.KindCoMarketing or RebateCodes.KindListing)
            .WithMessage(SupplierRebatesErrors.Kind);
        _ = RuleFor(x => x.Input.Description).NotEmpty().WithMessage(SupplierRebatesErrors.Required).MaximumLength(DescriptionLength).WithMessage(SupplierRebatesErrors.Required);
        _ = RuleFor(x => x.Input.Amount).GreaterThan(0m).WithMessage(SupplierRebatesErrors.Amount).LessThanOrEqualTo(MaxAmount).WithMessage(SupplierRebatesErrors.Amount);
    }
}
