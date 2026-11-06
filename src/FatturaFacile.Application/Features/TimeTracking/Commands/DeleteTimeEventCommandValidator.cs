// <copyright file="DeleteTimeEventCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.TimeTracking.Commands;

/// <summary>
/// Validator for <see cref="DeleteTimeEventCommand"/>.
/// </summary>
public sealed class DeleteTimeEventCommandValidator : AbstractValidator<DeleteTimeEventCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTimeEventCommandValidator"/> class.
    /// </summary>
    public DeleteTimeEventCommandValidator()
    {
        _ = RuleFor(x => x.Reason).NotEmpty().MinimumLength(3).WithMessage("Indicare il motivo dell'eliminazione (resta nello storico).");
    }
}
