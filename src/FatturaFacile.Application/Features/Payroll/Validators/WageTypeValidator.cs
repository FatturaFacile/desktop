// <copyright file="WageTypeValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Features.Payroll.Commands;
using FatturaFacile.Domain.Entities;
using FluentValidation;

namespace FatturaFacile.Application.Features.Payroll.Validators;

/// <summary>
/// Validates wage types.
/// </summary>
public sealed class WageTypeValidator : AbstractValidator<SavePayrollRecordCommand<PayrollWageType>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WageTypeValidator"/> class.
    /// </summary>
    public WageTypeValidator()
    {
        _ = RuleFor(x => x.Record.Code).NotEmpty().Matches("^[0-9A-Z]{1,6}$").WithMessage("Il codice deve avere da 1 a 6 cifre o lettere maiuscole.");
        _ = RuleFor(x => x.Record.Name).NotEmpty().WithMessage("Indicare la descrizione.");
        _ = RuleFor(x => x.Record.Kind).Must(k => PayrollValidation.WageTypeKinds.Contains(k, StringComparer.Ordinal)).WithMessage("Tipo di genere salariale non valido.");
    }
}
