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
    private const int MaxTenantNameLength = 150;
    private const int MaxTenantSlugLength = 50;
    private const int MaxEmailLength = 255;
    private const int MinPasswordLength = 8;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterTenantUserCommandValidator"/> class.
    /// </summary>
    public RegisterTenantUserCommandValidator()
    {
        _ = RuleFor(x => x.TenantName).NotEmpty().MaximumLength(MaxTenantNameLength);
        _ = RuleFor(x => x.TenantSlug).NotEmpty().MaximumLength(MaxTenantSlugLength);
        _ = RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(MaxEmailLength);
        _ = RuleFor(x => x.Password).NotEmpty().MinimumLength(MinPasswordLength).WithMessage("Password must be at least 8 characters long.");
    }
}
