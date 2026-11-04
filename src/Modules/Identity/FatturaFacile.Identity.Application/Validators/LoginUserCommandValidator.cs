// <copyright file="LoginUserCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Identity.Application.Commands;
using FluentValidation;

namespace FatturaFacile.Identity.Application.Validators;

/// <summary>
/// Validator for validating <see cref="LoginUserCommand"/> input properties.
/// </summary>
public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginUserCommandValidator"/> class.
    /// </summary>
    public LoginUserCommandValidator()
    {
        _ = RuleFor(x => x.TenantSlug).NotEmpty().WithMessage("TenantSlug is required.");
        _ = RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("A valid email address is required.");
        _ = RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}
