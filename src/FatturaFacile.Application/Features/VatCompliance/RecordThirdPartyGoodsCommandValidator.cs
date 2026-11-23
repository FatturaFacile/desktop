// <copyright file="RecordThirdPartyGoodsCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.VatCompliance;
using FluentValidation;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Validates <see cref="RecordThirdPartyGoodsCommand"/>; the messages are localization keys.
/// </summary>
public sealed class RecordThirdPartyGoodsCommandValidator : AbstractValidator<RecordThirdPartyGoodsCommand>
{
    private const int DocumentLength = 40;
    private const int NameLength = 160;
    private const int DescriptionLength = 500;
    private const decimal MaxAmount = 9_999_999_999m;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordThirdPartyGoodsCommandValidator"/> class.
    /// </summary>
    public RecordThirdPartyGoodsCommandValidator()
    {
        _ = RuleFor(x => x.DocumentNumber).NotEmpty().MaximumLength(DocumentLength).WithMessage(VatComplianceErrors.Required);
        _ = RuleFor(x => x.PartnerName).NotEmpty().MaximumLength(NameLength).WithMessage(VatComplianceErrors.Required);
        _ = RuleFor(x => x.Description).NotEmpty().MaximumLength(DescriptionLength).WithMessage(VatComplianceErrors.Required);
        _ = RuleFor(x => x.Value).NotEqual(0m).InclusiveBetween(-MaxAmount, MaxAmount).WithMessage(VatComplianceErrors.Amount);
    }
}
