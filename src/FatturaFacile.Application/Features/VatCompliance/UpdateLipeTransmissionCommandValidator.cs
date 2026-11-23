// <copyright file="UpdateLipeTransmissionCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Enums;
using FluentValidation;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Validates <see cref="UpdateLipeTransmissionCommand"/>; the messages are localization keys.
/// </summary>
public sealed class UpdateLipeTransmissionCommandValidator : AbstractValidator<UpdateLipeTransmissionCommand>
{
    private const int ReceiptLength = 500;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateLipeTransmissionCommandValidator"/> class.
    /// </summary>
    public UpdateLipeTransmissionCommandValidator()
    {
        _ = RuleFor(x => x.Status).Must(static s => s is LipeTransmissionStatus.Transmitted or LipeTransmissionStatus.Accepted or LipeTransmissionStatus.Rejected).WithMessage(VatComplianceErrors.SubmissionState);
        _ = RuleFor(x => x.ReceiptProtocol).MaximumLength(ReceiptLength).When(x => x.ReceiptProtocol is not null).WithMessage(VatComplianceErrors.Required);
        _ = RuleFor(x => x.ReceiptNote).MaximumLength(ReceiptLength).When(x => x.ReceiptNote is not null).WithMessage(VatComplianceErrors.Required);
    }
}
