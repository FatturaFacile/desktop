// <copyright file="QueueCreateProductionOrderCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Manufacturing.Features;

/// <summary>
/// Validates <see cref="QueueCreateProductionOrderCommand"/> with the same rules as the web-api request.
/// </summary>
public sealed class QueueCreateProductionOrderCommandValidator : AbstractValidator<QueueCreateProductionOrderCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueueCreateProductionOrderCommandValidator"/> class.
    /// </summary>
    public QueueCreateProductionOrderCommandValidator()
    {
        _ = RuleFor(c => c.ItemId).NotEmpty();
        _ = RuleFor(c => c.TargetQuantity).GreaterThan(0m);
        _ = RuleFor(c => c.PlannedEndDate).GreaterThanOrEqualTo(c => c.PlannedStartDate);
    }
}
