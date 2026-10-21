// <copyright file="LoginCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.Administration;

/// <summary>
/// Represents login command validator.
/// </summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginCommandValidator"/> class.
    /// </summary>
    public LoginCommandValidator()
    {
        _ = RuleFor(x => x.Username).NotEmpty();
        _ = RuleFor(x => x.Password).NotEmpty();
        _ = RuleFor(x => x.TenantId).NotEmpty();
    }
}
