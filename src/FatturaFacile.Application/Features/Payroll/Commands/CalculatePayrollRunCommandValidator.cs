// <copyright file="CalculatePayrollRunCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.Payroll.Commands;

/// <summary>
/// Validator for <see cref="CalculatePayrollRunCommand"/>.
/// </summary>
public sealed class CalculatePayrollRunCommandValidator : AbstractValidator<CalculatePayrollRunCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalculatePayrollRunCommandValidator"/> class.
    /// </summary>
    public CalculatePayrollRunCommandValidator()
    {
        _ = RuleFor(x => x.Year).InclusiveBetween(2000, 2100).WithMessage("Anno non valido.");
        _ = RuleFor(x => x.Month).InclusiveBetween(1, 12).WithMessage("Mese non valido.");
    }
}
