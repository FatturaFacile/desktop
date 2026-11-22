// <copyright file="CreateJournalEntryCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FluentValidation;

namespace FatturaFacile.Application.Features.Accounting.Commands;

/// <summary>
/// Validates a new Prima Nota entry: always the line rules, and the balance when it is posted at once.
/// </summary>
public sealed class CreateJournalEntryCommandValidator : AbstractValidator<CreateJournalEntryCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateJournalEntryCommandValidator"/> class.
    /// </summary>
    public CreateJournalEntryCommandValidator()
    {
        _ = RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required.");
        _ = RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line is required.");
        _ = RuleFor(x => x).Custom(static (command, context) =>
        {
            var blocking = command.PostImmediately
                ? JournalRules.Validate([.. command.Lines])
                : [.. JournalRules.Validate([.. command.Lines]).Where(static v => v is JournalViolation.NegativeAmount or JournalViolation.BothSides or JournalViolation.MissingAccount)];

            foreach (var violation in blocking)
            {
                context.AddFailure(nameof(CreateJournalEntryCommand.Lines), "Journal rule violated: " + violation);
            }
        });
    }
}
