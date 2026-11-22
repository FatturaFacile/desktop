// <copyright file="ConvertServiceReportToInvoiceCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Validator for <see cref="ConvertServiceReportToInvoiceCommand"/>. The messages are localization keys, resolved by the view.
/// </summary>
public sealed class ConvertServiceReportToInvoiceCommandValidator : AbstractValidator<ConvertServiceReportToInvoiceCommand>
{
    private const decimal MaxTaxRate = 100m;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertServiceReportToInvoiceCommandValidator"/> class.
    /// </summary>
    public ConvertServiceReportToInvoiceCommandValidator()
    {
        _ = RuleFor(x => x.ReportId).Must(id => Guid.TryParse(id, out var parsed) && parsed != Guid.Empty).WithMessage("FieldService_Err_Report");
        _ = RuleFor(x => x.TaxRate).InclusiveBetween(0m, MaxTaxRate).WithMessage("FieldService_Err_TaxRate");
    }
}
