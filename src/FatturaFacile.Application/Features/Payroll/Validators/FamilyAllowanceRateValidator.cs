// <copyright file="FamilyAllowanceRateValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Features.Payroll.Commands;
using FatturaFacile.Domain.Entities;
using FluentValidation;

namespace FatturaFacile.Application.Features.Payroll.Validators;

/// <summary>
/// Validates family allowance amounts.
/// </summary>
public sealed class FamilyAllowanceRateValidator : AbstractValidator<SavePayrollRecordCommand<FamilyAllowanceRate>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FamilyAllowanceRateValidator"/> class.
    /// </summary>
    public FamilyAllowanceRateValidator()
    {
        _ = RuleFor(x => x.Record.Canton).Must(PayrollValidation.IsCanton).WithMessage("Cantone non valido.");
        _ = RuleFor(x => x.Record.ChildAllowance).GreaterThanOrEqualTo(215m).WithMessage("L'assegno per figli non può essere inferiore al minimo federale (CHF 215).");
        _ = RuleFor(x => x.Record.EducationAllowance).GreaterThanOrEqualTo(268m).WithMessage("L'assegno di formazione non può essere inferiore al minimo federale (CHF 268).");
    }
}
