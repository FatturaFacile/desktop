// <copyright file="RecordWithholdingPaymentCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.VatCompliance;
using FluentValidation;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Validates <see cref="RecordWithholdingPaymentCommand"/>; the messages are localization keys.
/// </summary>
public sealed class RecordWithholdingPaymentCommandValidator : AbstractValidator<RecordWithholdingPaymentCommand>
{
    private const int ReferenceLength = 120;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordWithholdingPaymentCommandValidator"/> class.
    /// </summary>
    public RecordWithholdingPaymentCommandValidator()
    {
        _ = RuleFor(x => x.PaymentId).NotEmpty().WithMessage(VatComplianceErrors.PaymentNotFound);
        _ = RuleFor(x => x.PaymentReference).MaximumLength(ReferenceLength).When(x => x.PaymentReference is not null).WithMessage(VatComplianceErrors.Required);
    }
}
