// <copyright file="SaveTimeDailyRecordCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.TimeTracking.Commands;

/// <summary>
/// Validator for <see cref="SaveTimeDailyRecordCommand"/>.
/// </summary>
public sealed class SaveTimeDailyRecordCommandValidator : AbstractValidator<SaveTimeDailyRecordCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SaveTimeDailyRecordCommandValidator"/> class.
    /// </summary>
    public SaveTimeDailyRecordCommandValidator()
    {
        _ = RuleFor(x => x.Record).NotNull();
        _ = RuleFor(x => x.Record.EmployeeId).NotEqual(Guid.Empty).WithMessage("Selezionare il dipendente.");
        _ = RuleFor(x => x.Record.WorkedMinutes).InclusiveBetween(0, 24 * 60).WithMessage("Durata giornaliera non valida.");
        _ = RuleFor(x => x.Record.Date).LessThanOrEqualTo(_ => DateTime.Today).WithMessage("Non si possono registrare giorni futuri.");
        _ = RuleFor(x => x.Record.EndTime).GreaterThan(x => x.Record.StartTime).When(x => x.Record.StartTime is not null && x.Record.EndTime is not null)
            .WithMessage("La fine deve seguire l'inizio.");
    }
}
