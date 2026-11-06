// <copyright file="SaveTimeEventCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.TimeTracking;
using FluentValidation;

namespace FatturaFacile.Application.Features.TimeTracking.Commands;

/// <summary>
/// Validator for <see cref="SaveTimeEventCommand"/>.
/// </summary>
public sealed class SaveTimeEventCommandValidator : AbstractValidator<SaveTimeEventCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SaveTimeEventCommandValidator"/> class.
    /// </summary>
    public SaveTimeEventCommandValidator()
    {
        _ = RuleFor(x => x.Event).NotNull();
        _ = RuleFor(x => x.Event.EmployeeId).NotEqual(Guid.Empty).WithMessage("Selezionare il dipendente.");
        _ = RuleFor(x => x.Event.EventType).Must(t => TimeEventType.All.Contains(t, StringComparer.Ordinal)).WithMessage("Tipo di timbratura non valido.");
        _ = RuleFor(x => x.Event.Timestamp).LessThanOrEqualTo(_ => DateTime.Now.AddMinutes(5)).WithMessage("Non si possono registrare timbrature future.");
        _ = RuleFor(x => x.Reason).NotEmpty().MinimumLength(3).WithMessage("Indicare il motivo dell'inserimento o della correzione (resta nello storico).");
    }
}
