// <copyright file="CreateUserCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.Users.Commands;

/// <summary>
/// Validator for <see cref="CreateUserCommand"/>.
/// </summary>
public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateUserCommandValidator"/> class.
    /// </summary>
    public CreateUserCommandValidator()
    {
        _ = RuleFor(x => x.UserName).NotEmpty().MaximumLength(64).WithMessage("Il nome utente è obbligatorio (max 64 caratteri).");
        _ = RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(128).WithMessage("Il nome visualizzato è obbligatorio.");
        _ = RuleFor(x => x.Password).MinimumLength(12).When(x => x.Password is not null).WithMessage("La password deve avere almeno 12 caratteri.");
        _ = RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.WindowsAccountName) || !string.IsNullOrEmpty(x.Password))
            .WithMessage("Indicare un account Windows o una password.");
    }
}
