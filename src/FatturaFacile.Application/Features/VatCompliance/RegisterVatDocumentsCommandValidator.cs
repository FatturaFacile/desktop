// <copyright file="RegisterVatDocumentsCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.VatCompliance;
using FluentValidation;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Validates <see cref="RegisterVatDocumentsCommand"/>; the messages are localization keys.
/// </summary>
public sealed class RegisterVatDocumentsCommandValidator : AbstractValidator<RegisterVatDocumentsCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterVatDocumentsCommandValidator"/> class.
    /// </summary>
    public RegisterVatDocumentsCommandValidator()
    {
        _ = RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).WithMessage(VatComplianceErrors.DatesOrder);
        _ = RuleFor(x => x.To.Year).Equal(x => x.From.Year).WithMessage(VatComplianceErrors.DatesOrder);
        _ = RuleFor(x => x.ProRataPercent).InclusiveBetween(0m, 100m).When(x => x.ProRataPercent is not null).WithMessage(VatComplianceErrors.Percent);
    }
}
