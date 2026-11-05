// <copyright file="CreateInvoiceCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.Invoicing.Commands;

/// <summary>
/// Validator for <see cref="CreateInvoiceCommand"/>.
/// </summary>
public sealed class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateInvoiceCommandValidator"/> class.
    /// </summary>
    public CreateInvoiceCommandValidator()
    {
        _ = RuleFor(x => x.ContactId).NotEmpty().WithMessage("Contact must be specified.");
        _ = RuleFor(x => x.InvoiceNumber).NotEmpty().MaximumLength(50).WithMessage("Invoice number is required.");
        _ = RuleFor(x => x.Date).NotEmpty().WithMessage("Invoice date is required.");
        _ = RuleFor(x => x.Items).NotEmpty().WithMessage("At least one line item is required.");
    }
}
