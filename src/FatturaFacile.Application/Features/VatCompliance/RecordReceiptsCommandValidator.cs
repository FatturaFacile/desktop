// <copyright file="RecordReceiptsCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.VatCompliance;
using FluentValidation;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Validates <see cref="RecordReceiptsCommand"/>; the messages are localization keys.
/// </summary>
public sealed class RecordReceiptsCommandValidator : AbstractValidator<RecordReceiptsCommand>
{
    private const decimal MaxAmount = 9_999_999_999m;
    private const decimal RateTolerance = 0.01m;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordReceiptsCommandValidator"/> class.
    /// </summary>
    public RecordReceiptsCommandValidator()
    {
        _ = RuleFor(x => x.GrossAmount).GreaterThan(0m).LessThanOrEqualTo(MaxAmount).WithMessage(VatComplianceErrors.Amount);
        _ = RuleForEach(x => x.Allocations).ChildRules(static a =>
        {
            _ = a.RuleFor(l => l.VatRate).InclusiveBetween(0m, 100m).WithMessage(VatComplianceErrors.Amount);
            _ = a.RuleFor(l => l.Gross).GreaterThan(0m).LessThanOrEqualTo(MaxAmount).WithMessage(VatComplianceErrors.Amount);
        });
        _ = RuleFor(x => x.Allocations).Must(static (command, allocations) => allocations.Count == 0 || Math.Abs(allocations.Sum(static a => a.Gross) - command.GrossAmount) <= RateTolerance).WithMessage(VatComplianceErrors.Amount);
    }
}
