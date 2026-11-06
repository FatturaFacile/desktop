// <copyright file="SetUserPasswordCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.Users.Commands;

/// <summary>
/// Validator for <see cref="SetUserPasswordCommand"/>.
/// </summary>
public sealed class SetUserPasswordCommandValidator : AbstractValidator<SetUserPasswordCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetUserPasswordCommandValidator"/> class.
    /// </summary>
    public SetUserPasswordCommandValidator()
    {
        _ = RuleFor(x => x.NewPassword).MinimumLength(12).WithMessage("La password deve avere almeno 12 caratteri.");
    }
}
