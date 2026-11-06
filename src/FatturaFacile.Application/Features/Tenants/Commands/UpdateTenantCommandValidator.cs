// <copyright file="UpdateTenantCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Validation;
using FluentValidation;

namespace FatturaFacile.Application.Features.Tenants.Commands;

/// <summary>
/// Validator for <see cref="UpdateTenantCommand"/>.
/// </summary>
public sealed class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTenantCommandValidator"/> class.
    /// </summary>
    public UpdateTenantCommandValidator()
    {
        _ = RuleFor(x => x.Tenant.Name).NotEmpty().MaximumLength(255).WithMessage("Il nome breve dell'azienda è obbligatorio.");
        _ = RuleFor(x => x.Tenant.LegalName).NotEmpty().MaximumLength(255).WithMessage("La ragione sociale è obbligatoria.");
        _ = RuleFor(x => x.Tenant.Iban)
            .Must(static iban => string.IsNullOrWhiteSpace(iban) || IbanValidator.IsValid(iban))
            .WithMessage("IBAN non valido.");
        _ = RuleFor(x => x.Tenant.QrIban)
            .Must(static iban => string.IsNullOrWhiteSpace(iban) || IbanValidator.IsSwissQrIban(iban))
            .WithMessage("Il QR-IBAN non è valido (deve essere un QR-IBAN svizzero, IID 30000-31999).");
        _ = RuleFor(x => x.Tenant.Province)
            .Must(static p => string.IsNullOrWhiteSpace(p) || p.Trim().Length == 2)
            .WithMessage("La provincia/cantone deve essere una sigla di 2 lettere.");
        _ = RuleFor(x => x.Tenant.TaxRegime)
            .Matches("^RF(0[1-9]|1[0-9])$")
            .WithMessage("Regime fiscale non valido (RF01..RF19).");
    }
}
