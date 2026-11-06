// <copyright file="AbsenceValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Features.Payroll.Commands;
using FatturaFacile.Domain.Entities;
using FluentValidation;

namespace FatturaFacile.Application.Features.Payroll.Validators;

/// <summary>
/// Validates absences.
/// </summary>
public sealed class AbsenceValidator : AbstractValidator<SavePayrollRecordCommand<HrAbsence>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AbsenceValidator"/> class.
    /// </summary>
    public AbsenceValidator()
    {
        _ = RuleFor(x => x.Record.EmployeeId).NotEqual(Guid.Empty).WithMessage("Selezionare il dipendente.");
        _ = RuleFor(x => x.Record.EndDate).GreaterThanOrEqualTo(x => x.Record.StartDate).WithMessage("La fine precede l'inizio dell'assenza.");
        _ = RuleFor(x => x.Record.Percent).InclusiveBetween(1m, 100m).WithMessage("La percentuale deve essere tra 1 e 100.");
        _ = RuleFor(x => x.Record.AbsenceType).Must(t => PayrollValidation.AbsenceTypes.Contains(t, StringComparer.Ordinal)).WithMessage("Tipo di assenza non valido.");
    }
}
