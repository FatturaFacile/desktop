// <copyright file="EmployeeChildValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Features.Payroll.Commands;
using FatturaFacile.Domain.Entities;
using FluentValidation;

namespace FatturaFacile.Application.Features.Payroll.Validators;

/// <summary>
/// Validates children of employees.
/// </summary>
public sealed class EmployeeChildValidator : AbstractValidator<SavePayrollRecordCommand<HrEmployeeChild>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeChildValidator"/> class.
    /// </summary>
    public EmployeeChildValidator()
    {
        _ = RuleFor(x => x.Record.EmployeeId).NotEqual(Guid.Empty).WithMessage("Selezionare il dipendente.");
        _ = RuleFor(x => x.Record.FirstName).NotEmpty().WithMessage("Indicare il nome del figlio.");
        _ = RuleFor(x => x.Record.DateOfBirth).LessThanOrEqualTo(_ => DateTime.Today.AddMonths(9)).WithMessage("Data di nascita non valida.");
    }
}
