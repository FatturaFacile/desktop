// <copyright file="EmployeeValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Features.Payroll.Commands;
using FatturaFacile.Domain.Entities;
using FluentValidation;

namespace FatturaFacile.Application.Features.Payroll.Validators;

/// <summary>
/// Validates employee master data (Swiss and Italian payroll).
/// </summary>
public sealed class EmployeeValidator : AbstractValidator<SavePayrollRecordCommand<HrEmployee>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeValidator"/> class.
    /// </summary>
    public EmployeeValidator()
    {
        _ = RuleFor(x => x.Record.EmployeeNumber).NotEmpty().WithMessage("Indicare il numero del dipendente.");
        _ = RuleFor(x => x.Record.FirstName).NotEmpty().WithMessage("Indicare il nome.");
        _ = RuleFor(x => x.Record.LastName).NotEmpty().WithMessage("Indicare il cognome.");
        _ = RuleFor(x => x.Record.BaseAnnualSalary).GreaterThanOrEqualTo(0m).WithMessage("Il salario annuo non può essere negativo.");
        _ = RuleFor(x => x.Record.HourlyWage).GreaterThanOrEqualTo(0m).WithMessage("Il salario orario non può essere negativo.");
        _ = RuleFor(x => x.Record.SalaryInstallmentsPerYear).InclusiveBetween(12, 14).WithMessage("Le mensilità devono essere 12, 13 o 14.");
        _ = RuleFor(x => x.Record.TerminationDate).GreaterThanOrEqualTo(x => x.Record.HireDate).When(x => x.Record.TerminationDate is not null)
            .WithMessage("La data di uscita precede la data di entrata.");
        _ = RuleFor(x => x.Record.SocialSecurityNumber).Matches(@"^756\.?\d{4}\.?\d{4}\.?\d{2}$")
            .When(x => string.Equals(x.Record.Currency, "CHF", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(x.Record.SocialSecurityNumber))
            .WithMessage("Il numero AVS deve avere il formato 756.XXXX.XXXX.XX.");
        _ = RuleFor(x => x.Record.WithholdingTaxCode).NotEmpty().Matches("^[A-Z][0-9][NY]$").When(x => x.Record.IsWithholdingTaxSubject)
            .WithMessage("Codice tariffa dell'imposta alla fonte non valido (es. A0N, B2N, C1Y).");
        _ = RuleFor(x => x.Record.ResidencePermit).Must(p => p is null || PayrollValidation.Permits.Contains(p, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Permesso non valido (CH, C, B, L, G, F, N, S, Ci).");
        _ = RuleFor(x => x.Record.WorkCanton).Must(PayrollValidation.IsCanton).When(x => !string.IsNullOrWhiteSpace(x.Record.WorkCanton))
            .WithMessage("Cantone di lavoro non valido.");
    }
}
