// <copyright file="DuplicateReportTemplateCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.Reports.Commands;

/// <summary>
/// Validator for <see cref="DuplicateReportTemplateCommand"/>: the new name must be a plain file name.
/// </summary>
public sealed class DuplicateReportTemplateCommandValidator : AbstractValidator<DuplicateReportTemplateCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateReportTemplateCommandValidator"/> class.
    /// </summary>
    public DuplicateReportTemplateCommandValidator()
    {
        _ = RuleFor(x => x.SourceName).NotEmpty().WithMessage("Selezionare il modello da duplicare.");
        _ = RuleFor(x => x.NewName)
            .NotEmpty()
            .MaximumLength(80)
            .Matches("^[A-Za-z0-9][A-Za-z0-9 _.-]*$")
            .WithMessage("Il nome del modello può contenere solo lettere, cifre, spazi, '_', '-' e '.' (es. Fattura_IT_Logo).");
    }
}
