// <copyright file="CreateQuoteCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.Sales.Commands;

/// <summary>
/// Validator for <see cref="CreateQuoteCommand"/>.
/// </summary>
public sealed class CreateQuoteCommandValidator : AbstractValidator<CreateQuoteCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateQuoteCommandValidator"/> class.
    /// </summary>
    public CreateQuoteCommandValidator()
    {
        _ = RuleFor(x => x.ContactId).NotEmpty().WithMessage("Contact must be specified.");
        _ = RuleFor(x => x.Items).NotEmpty().WithMessage("At least one line item is required.");
        _ = RuleFor(x => x).Must(static x => x.ValidUntil is null || x.ValidUntil.Value.Date >= x.Date.Date)
            .WithMessage("The validity date cannot precede the quote date.");
        _ = RuleForEach(x => x.Items).ChildRules(line =>
        {
            _ = line.RuleFor(l => l.Description).NotEmpty().MaximumLength(255);
            _ = line.RuleFor(l => l.Quantity).GreaterThan(0m);
            _ = line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0m);
            _ = line.RuleFor(l => l.TaxRate).InclusiveBetween(0m, 100m);
            _ = line.RuleFor(l => l.DiscountPercentage).InclusiveBetween(0m, 100m);
        });
    }
}
