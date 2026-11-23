// <copyright file="SaveIntentLetterCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.VatCompliance;
using FluentValidation;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Validates <see cref="SaveIntentLetterCommand"/>; the messages are localization keys.
/// </summary>
public sealed class SaveIntentLetterCommandValidator : AbstractValidator<SaveIntentLetterCommand>
{
    private const int NotesLength = 2000;
    private const decimal MaxAmount = 9_999_999_999m;
    private const int FirstYear = 2000;
    private const int LastYear = 2100;
    private const string ProtocolPattern = "^[0-9]{17}-[0-9]{6}$";

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveIntentLetterCommandValidator"/> class.
    /// </summary>
    public SaveIntentLetterCommandValidator()
    {
        _ = RuleFor(x => x.Input.ContactId).NotEmpty().WithMessage(VatComplianceErrors.Required);
        _ = RuleFor(x => x.Input.ProtocolNumber).NotEmpty().WithMessage(VatComplianceErrors.Required).Matches(ProtocolPattern).WithMessage(VatComplianceErrors.ProtocolFormat);
        _ = RuleFor(x => x.Input.PlafondAmount).GreaterThan(0m).LessThanOrEqualTo(MaxAmount).WithMessage(VatComplianceErrors.Amount);
        _ = RuleFor(x => x.Input.Year).InclusiveBetween(FirstYear, LastYear).WithMessage(VatComplianceErrors.Amount);
        _ = RuleFor(x => x.Input.ValidTo).GreaterThanOrEqualTo(x => x.Input.ValidFrom).WithMessage(VatComplianceErrors.DatesOrder);
        _ = RuleFor(x => x.Input.Notes).MaximumLength(NotesLength).When(x => x.Input.Notes is not null).WithMessage(VatComplianceErrors.Required);
    }
}
