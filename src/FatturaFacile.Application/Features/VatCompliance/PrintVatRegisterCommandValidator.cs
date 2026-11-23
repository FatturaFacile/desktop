// <copyright file="PrintVatRegisterCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.VatCompliance;
using FluentValidation;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Validates <see cref="PrintVatRegisterCommand"/>; the messages are localization keys.
/// </summary>
public sealed class PrintVatRegisterCommandValidator : AbstractValidator<PrintVatRegisterCommand>
{
    private const int ReferenceLength = 80;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrintVatRegisterCommandValidator"/> class.
    /// </summary>
    public PrintVatRegisterCommandValidator()
    {
        _ = RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).WithMessage(VatComplianceErrors.DatesOrder);
        _ = RuleFor(x => x.From.Year).Equal(x => x.Year).WithMessage(VatComplianceErrors.DatesOrder);
        _ = RuleFor(x => x.To.Year).Equal(x => x.Year).WithMessage(VatComplianceErrors.DatesOrder);
        _ = RuleFor(x => x.VirtualStampReference).MaximumLength(ReferenceLength).When(x => x.VirtualStampReference is not null).WithMessage(VatComplianceErrors.Required);
    }
}
