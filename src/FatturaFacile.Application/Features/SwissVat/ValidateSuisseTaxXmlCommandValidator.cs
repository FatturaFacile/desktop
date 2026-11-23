// <copyright file="ValidateSuisseTaxXmlCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.SwissVat;

/// <summary>
/// Validates <see cref="ValidateSuisseTaxXmlCommand"/>.
/// </summary>
public sealed class ValidateSuisseTaxXmlCommandValidator : AbstractValidator<ValidateSuisseTaxXmlCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateSuisseTaxXmlCommandValidator"/> class.
    /// </summary>
    public ValidateSuisseTaxXmlCommandValidator()
    {
        _ = RuleFor(x => x.Result).NotNull();
        _ = RuleFor(x => x.CompanyName).NotEmpty();
    }
}
