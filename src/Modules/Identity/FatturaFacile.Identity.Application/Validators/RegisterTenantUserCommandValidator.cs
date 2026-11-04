// <copyright file="RegisterTenantUserCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Identity.Application.Commands;
using FluentValidation;

namespace FatturaFacile.Identity.Application.Validators;

/// <summary>
/// Validator for validating <see cref="RegisterTenantUserCommand"/> input parameters.
/// </summary>
public sealed class RegisterTenantUserCommandValidator : AbstractValidator<RegisterTenantUserCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterTenantUserCommandValidator"/> class.
    /// </summary>
    public RegisterTenantUserCommandValidator()
    {
        _ = RuleFor(x => x.TenantName).NotEmpty().MaximumLength(150);
        _ = RuleFor(x => x.TenantSlug).NotEmpty().MaximumLength(50);
        _ = RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        _ = RuleFor(x => x.Password).NotEmpty().MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
    }
}
