// <copyright file="CreateEmployeeCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.Payroll.Commands;

/// <summary>
/// Validator for <see cref="CreateEmployeeCommand"/>.
/// </summary>
public sealed class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateEmployeeCommandValidator"/> class.
    /// </summary>
    public CreateEmployeeCommandValidator()
    {
        _ = RuleFor(x => x.EmployeeNumber).NotEmpty().WithMessage("Employee number is required.");
        _ = RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.");
        _ = RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required.");
        _ = RuleFor(x => x.BaseAnnualSalary).GreaterThanOrEqualTo(0).WithMessage("Base annual salary cannot be negative.");
    }
}
