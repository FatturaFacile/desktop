// <copyright file="PayrollSettingsValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Features.Payroll.Commands;
using FatturaFacile.Domain.Entities;
using FluentValidation;

namespace FatturaFacile.Application.Features.Payroll.Validators;

/// <summary>
/// Validates the Swiss payroll settings.
/// </summary>
public sealed class PayrollSettingsValidator : AbstractValidator<SavePayrollRecordCommand<SwissPayrollSettings>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PayrollSettingsValidator"/> class.
    /// </summary>
    public PayrollSettingsValidator()
    {
        _ = RuleFor(x => x.Record.BvgEmployerSharePercent).InclusiveBetween(50m, 100m).WithMessage("Il datore di lavoro deve pagare almeno il 50 % dei contributi LPP.");
        _ = RuleFor(x => x.Record.AhvEmployeeRatePercent).InclusiveBetween(0m, 20m).WithMessage("Aliquota AVS non valida.");
        _ = RuleFor(x => x.Record.AlvAnnualCeiling).GreaterThan(0m).WithMessage("Indicare il tetto AD.");
        _ = RuleFor(x => x.Record.UvgAnnualCeiling).GreaterThan(0m).WithMessage("Indicare il salario massimo LAINF.");
        _ = RuleFor(x => x.Record.DefaultWorkCanton).Must(PayrollValidation.IsCanton).WithMessage("Cantone non valido.");
    }
}
