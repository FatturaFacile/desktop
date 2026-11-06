// <copyright file="CreateGeneralLedgerEntryCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.Accounting.Commands;

/// <summary>
/// Validator for <see cref="CreateGeneralLedgerEntryCommand"/>.
/// </summary>
public sealed class CreateGeneralLedgerEntryCommandValidator : AbstractValidator<CreateGeneralLedgerEntryCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateGeneralLedgerEntryCommandValidator"/> class.
    /// </summary>
    public CreateGeneralLedgerEntryCommandValidator()
    {
        _ = RuleFor(x => x.EntryNumber).NotEmpty().WithMessage("Entry number is required.");
        _ = RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required.");
        _ = RuleFor(x => x.Lines).NotEmpty().Must(l => l.Count >= 2).WithMessage("At least two lines (one debit, one credit) are required.");
        _ = RuleFor(x => x).Must(BeBalanced).WithMessage("The entry is not balanced. Total Debit must equal Total Credit.");
    }

    private static bool BeBalanced(CreateGeneralLedgerEntryCommand cmd)
    {
        if (cmd.Lines == null || cmd.Lines.Count == 0)
        {
            return false;
        }

        var debit = cmd.Lines.Sum(l => l.Debit);
        var credit = cmd.Lines.Sum(l => l.Credit);
        return Math.Abs(debit - credit) < 0.001m;
    }
}
