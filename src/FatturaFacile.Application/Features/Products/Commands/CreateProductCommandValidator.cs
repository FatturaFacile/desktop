// <copyright file="CreateProductCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.Products.Commands;

/// <summary>
/// Validator for <see cref="CreateProductCommand"/>.
/// </summary>
public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateProductCommandValidator"/> class.
    /// </summary>
    public CreateProductCommandValidator()
    {
        _ = RuleFor(x => x.Sku).NotEmpty().MaximumLength(50).WithMessage("SKU is required.");
        _ = RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("Product name is required.");
        _ = RuleFor(x => x.SalePrice).GreaterThanOrEqualTo(0).WithMessage("Sale price must not be negative.");
    }
}
