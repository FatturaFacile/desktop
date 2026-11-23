// <copyright file="SaveRebateAgreementCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using FluentValidation;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Validator for <see cref="SaveRebateAgreementCommand"/>: the same rules as the web-api, so an agreement the server would reject
/// is refused on the spot. The messages are localization keys.
/// </summary>
public sealed class SaveRebateAgreementCommandValidator : AbstractValidator<SaveRebateAgreementCommand>
{
    private const int CodeLength = 40;
    private const int NameLength = 160;
    private const int CurrencyLength = 3;
    private const int NotesLength = 2000;
    private const int MaxTiers = 20;
    private const decimal MaxAmount = 9_999_999_999_999m;
    private const decimal MaxPercent = 100m;

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveRebateAgreementCommandValidator"/> class.
    /// </summary>
    public SaveRebateAgreementCommandValidator()
    {
        _ = RuleFor(x => x.Input.Code).NotEmpty().WithMessage(SupplierRebatesErrors.Required).MaximumLength(CodeLength).WithMessage(SupplierRebatesErrors.Required);
        _ = RuleFor(x => x.Input.Name).NotEmpty().WithMessage(SupplierRebatesErrors.Required).MaximumLength(NameLength).WithMessage(SupplierRebatesErrors.Required);
        _ = RuleFor(x => x.Input.ContactId).NotEmpty().WithMessage(SupplierRebatesErrors.Required);
        _ = RuleFor(x => x.Input.Currency).Length(CurrencyLength).WithMessage(SupplierRebatesErrors.Currency);
        _ = RuleFor(x => x.Input.Notes).MaximumLength(NotesLength).When(x => x.Input.Notes is not null).WithMessage(SupplierRebatesErrors.Required);
        _ = RuleFor(x => x.Input.EndsOn).GreaterThanOrEqualTo(x => x.Input.StartsOn).WithMessage(SupplierRebatesErrors.DatesOrder);
        _ = RuleFor(x => x.Input.RebateType)
            .Must(type => IsOneOf(type, RebateCodes.TypeTieredVolume, RebateCodes.TypeFixedBonus, RebateCodes.TypeMarketingContribution))
            .WithMessage(SupplierRebatesErrors.Required);
        _ = RuleFor(x => x.Input.AccrualFrequency)
            .Must(frequency => IsOneOf(frequency, RebateCodes.FrequencyMonthly, RebateCodes.FrequencyQuarterly, RebateCodes.FrequencyCustom))
            .WithMessage(SupplierRebatesErrors.Required);

        _ = RuleFor(x => x.Input.Tiers).NotEmpty().When(IsTiered).WithMessage(SupplierRebatesErrors.TiersRequired);
        _ = RuleFor(x => x.Input.Tiers).Must(tiers => tiers.Count <= MaxTiers).WithMessage(SupplierRebatesErrors.Amount);
        _ = RuleFor(x => x.Input.Tiers)
            .Must(tiers => tiers.Select(tier => RebateTierCalculator.Round(tier.ThresholdAmount)).ToHashSet().Count == tiers.Count)
            .WithMessage(SupplierRebatesErrors.TiersDistinct);
        _ = RuleForEach(x => x.Input.Tiers).ChildRules(static tier =>
        {
            _ = tier.RuleFor(t => t.ThresholdAmount).InclusiveBetween(0m, MaxAmount).WithMessage(SupplierRebatesErrors.Amount);
            _ = tier.RuleFor(t => t.RatePercent).InclusiveBetween(0m, MaxPercent).WithMessage(SupplierRebatesErrors.Amount);
            _ = tier.RuleFor(t => t.FlatAmount).InclusiveBetween(0m, MaxAmount).When(t => t.FlatAmount is not null).WithMessage(SupplierRebatesErrors.Amount);
        });

        _ = RuleFor(x => x.Input.FixedBonusTarget).NotNull().When(IsFixedBonus).WithMessage(SupplierRebatesErrors.BonusRequired);
        _ = RuleFor(x => x.Input.FixedBonusAmount).NotNull().When(IsFixedBonus).WithMessage(SupplierRebatesErrors.BonusRequired);
        _ = RuleFor(x => x.Input.FixedBonusTarget).InclusiveBetween(0m, MaxAmount).When(x => x.Input.FixedBonusTarget is not null).WithMessage(SupplierRebatesErrors.Amount);
        _ = RuleFor(x => x.Input.FixedBonusAmount).InclusiveBetween(0m, MaxAmount).When(x => x.Input.FixedBonusAmount is not null).WithMessage(SupplierRebatesErrors.Amount);
    }

    private static bool IsTiered(SaveRebateAgreementCommand command) => string.Equals(command.Input.RebateType, RebateCodes.TypeTieredVolume, StringComparison.Ordinal);

    private static bool IsFixedBonus(SaveRebateAgreementCommand command) => string.Equals(command.Input.RebateType, RebateCodes.TypeFixedBonus, StringComparison.Ordinal);

    private static bool IsOneOf(string? value, string first, string second, string third) =>
        string.Equals(value, first, StringComparison.Ordinal) || string.Equals(value, second, StringComparison.Ordinal) || string.Equals(value, third, StringComparison.Ordinal);
}
