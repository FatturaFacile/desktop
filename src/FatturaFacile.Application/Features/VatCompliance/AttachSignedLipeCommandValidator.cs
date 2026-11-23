// <copyright file="AttachSignedLipeCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.VatCompliance;
using FluentValidation;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Validates <see cref="AttachSignedLipeCommand"/>; the messages are localization keys.
/// </summary>
public sealed class AttachSignedLipeCommandValidator : AbstractValidator<AttachSignedLipeCommand>
{
    private const int FileNameLength = 255;

    /// <summary>
    /// Initializes a new instance of the <see cref="AttachSignedLipeCommandValidator"/> class.
    /// </summary>
    public AttachSignedLipeCommandValidator()
    {
        _ = RuleFor(x => x.FileName).NotEmpty().MaximumLength(FileNameLength).WithMessage(VatComplianceErrors.Required);
        _ = RuleFor(x => x.SignedContent.Length).GreaterThan(0).WithMessage(VatComplianceErrors.Required);
    }
}
