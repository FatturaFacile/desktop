// <copyright file="QueueOperationConfirmationCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Manufacturing.Features;

/// <summary>
/// Validates <see cref="QueueOperationConfirmationCommand"/> with the same rules as the web-api ConfirmOperationRequest.
/// </summary>
public sealed class QueueOperationConfirmationCommandValidator : AbstractValidator<QueueOperationConfirmationCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueueOperationConfirmationCommandValidator"/> class.
    /// </summary>
    public QueueOperationConfirmationCommandValidator()
    {
        _ = RuleFor(c => c.Input).NotNull();
        _ = RuleFor(c => c.Input.OrderId).NotEmpty();
        _ = RuleFor(c => c.Input.OperationId).NotEmpty();
        _ = RuleFor(c => c.Input.SetupMinutes).GreaterThanOrEqualTo(0m);
        _ = RuleFor(c => c.Input.RunMinutes).GreaterThanOrEqualTo(0m);
        _ = RuleFor(c => c.Input.LaborMinutes).GreaterThanOrEqualTo(0m);
        _ = RuleFor(c => c.Input.DowntimeMinutes).GreaterThanOrEqualTo(0m);
        _ = RuleFor(c => c.Input.GoodQuantity).GreaterThanOrEqualTo(0m);
        _ = RuleFor(c => c.Input.ScrapQuantity).GreaterThanOrEqualTo(0m);
        _ = RuleFor(c => c.Input.ScrapReason).NotEmpty().When(c => c.Input.ScrapQuantity > 0m).MaximumLength(255);
        _ = RuleFor(c => c.Input).Must(i => i.GoodQuantity > 0m || i.ScrapQuantity > 0m || i.RunMinutes > 0m || i.SetupMinutes > 0m)
            .WithMessage("A confirmation needs quantity or time.");
    }
}
