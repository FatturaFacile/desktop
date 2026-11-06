// <copyright file="CreateFirstAdministratorCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.Users.Commands;

/// <summary>
/// Validator for <see cref="CreateFirstAdministratorCommand"/>.
/// </summary>
public sealed class CreateFirstAdministratorCommandValidator : AbstractValidator<CreateFirstAdministratorCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateFirstAdministratorCommandValidator"/> class.
    /// </summary>
    public CreateFirstAdministratorCommandValidator()
    {
        _ = RuleFor(x => x.UserName).NotEmpty().MaximumLength(64).WithMessage("Il nome utente è obbligatorio (max 64 caratteri).");
        _ = RuleFor(x => x.Password).MinimumLength(12).WithMessage("La password deve avere almeno 12 caratteri.");
    }
}
