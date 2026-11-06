// <copyright file="CreateBackupCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.Backup.Commands;

/// <summary>
/// Validator for <see cref="CreateBackupCommand"/>.
/// </summary>
public sealed class CreateBackupCommandValidator : AbstractValidator<CreateBackupCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateBackupCommandValidator"/> class.
    /// </summary>
    public CreateBackupCommandValidator()
    {
        _ = RuleFor(x => x.ZipPath).NotEmpty().WithMessage("Indicare il file di destinazione del backup.");
        _ = RuleFor(x => x.Password).MinimumLength(8).WithMessage("La password del backup deve avere almeno 8 caratteri.");
    }
}
