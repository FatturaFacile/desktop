// <copyright file="CreateInvoiceCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Invoicing.Application.Commands;
using FatturaFacile.Invoicing.Domain.Repositories;
using FluentValidation;

namespace FatturaFacile.Invoicing.Application.Validators;

/// <summary>
/// Validator for enforcing business rules and structural constraints on <see cref="CreateInvoiceCommand"/>.
/// </summary>
public sealed class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    private const int MaxInvoiceNumberLength = 50;
    private const int CurrencyIsoCodeLength = 3;
    private const int MaxDescriptionLength = 1000;
    private const decimal MinPercentage = 0m;
    private const decimal MaxPercentage = 100m;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateInvoiceCommandValidator"/> class.
    /// </summary>
    /// <param name="invoiceRepository">The invoice repository for asynchronous uniqueness verification.</param>
    public CreateInvoiceCommandValidator(IInvoiceRepository invoiceRepository)
    {
        _ = RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId is mandatory.");

        _ = RuleFor(x => x.ContactId)
            .NotEmpty().WithMessage("ContactId is mandatory.");

        _ = RuleFor(x => x.InvoiceNumber)
            .NotEmpty().WithMessage("InvoiceNumber is required.")
            .MaximumLength(MaxInvoiceNumberLength).WithMessage("InvoiceNumber cannot exceed 50 characters.")
            .MustAsync(async (cmd, number, ct) =>
                !await invoiceRepository.ExistsAsync(cmd.TenantId, number, ct))
            .WithMessage("Invoice number already exists for this tenant.");

        _ = RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(CurrencyIsoCodeLength).WithMessage("Currency must be a 3-character ISO code.");

        _ = RuleFor(x => x.DueDate)
            .Must((cmd, dueDate) => !dueDate.HasValue || dueDate.Value >= cmd.Date)
            .WithMessage("DueDate must be greater than or equal to invoice Date.");

        _ = RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one invoice item is required.");

        _ = RuleForEach(x => x.Items).ChildRules(item =>
        {
            _ = item.RuleFor(i => i.Description)
                .NotEmpty().WithMessage("Item description is required.")
                .MaximumLength(MaxDescriptionLength);

            _ = item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            _ = item.RuleFor(i => i.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.");

            _ = item.RuleFor(i => i.TaxRate)
                .InclusiveBetween(MinPercentage, MaxPercentage).WithMessage("Tax rate must be between 0 and 100.");

            _ = item.RuleFor(i => i.DiscountPercentage)
                .InclusiveBetween(MinPercentage, MaxPercentage).WithMessage("Discount percentage must be between 0 and 100.");
        });
    }
}
