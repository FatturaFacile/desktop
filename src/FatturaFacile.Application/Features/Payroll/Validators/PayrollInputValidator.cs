// <copyright file="PayrollInputValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Features.Payroll.Commands;
using FatturaFacile.Domain.Entities;
using FluentValidation;

namespace FatturaFacile.Application.Features.Payroll.Validators;

/// <summary>
/// Validates monthly payroll inputs.
/// </summary>
public sealed class PayrollInputValidator : AbstractValidator<SavePayrollRecordCommand<PayrollInput>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PayrollInputValidator"/> class.
    /// </summary>
    public PayrollInputValidator()
    {
        _ = RuleFor(x => x.Record.EmployeeId).NotEqual(Guid.Empty).WithMessage("Selezionare il dipendente.");
        _ = RuleFor(x => x.Record.WageTypeCode).NotEmpty().WithMessage("Selezionare il genere salariale.");
        _ = RuleFor(x => x.Record.Month).InclusiveBetween(1, 12).WithMessage("Mese non valido.");
        _ = RuleFor(x => x.Record.Year).InclusiveBetween(2000, 2100).WithMessage("Anno non valido.");
        _ = RuleFor(x => x.Record).Must(r => r.Amount != 0m || r.Quantity != 0m).WithMessage("Indicare quantità o importo.");
    }
}
