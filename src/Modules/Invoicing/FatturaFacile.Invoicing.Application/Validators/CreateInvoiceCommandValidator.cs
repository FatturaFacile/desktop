using FluentValidation;
using FatturaFacile.Invoicing.Application.Commands;
using FatturaFacile.Invoicing.Domain.Repositories;

namespace FatturaFacile.Invoicing.Application.Validators;

public sealed class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator(IInvoiceRepository invoiceRepository)
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId is mandatory.");

        RuleFor(x => x.ContactId)
            .NotEmpty().WithMessage("ContactId is mandatory.");

        RuleFor(x => x.InvoiceNumber)
            .NotEmpty().WithMessage("InvoiceNumber is required.")
            .MaximumLength(50).WithMessage("InvoiceNumber cannot exceed 50 characters.")
            .MustAsync(async (cmd, number, ct) => 
                !await invoiceRepository.ExistsAsync(cmd.TenantId, number, ct))
            .WithMessage("Invoice number already exists for this tenant.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-character ISO code.");

        RuleFor(x => x.DueDate)
            .Must((cmd, dueDate) => !dueDate.HasValue || dueDate.Value >= cmd.Date)
            .WithMessage("DueDate must be greater than or equal to invoice Date.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one invoice item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Description)
                .NotEmpty().WithMessage("Item description is required.")
                .MaximumLength(1000);

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            item.RuleFor(i => i.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.");

            item.RuleFor(i => i.TaxRate)
                .InclusiveBetween(0, 100).WithMessage("Tax rate must be between 0 and 100.");

            item.RuleFor(i => i.DiscountPercentage)
                .InclusiveBetween(0, 100).WithMessage("Discount percentage must be between 0 and 100.");
        });
    }
}
