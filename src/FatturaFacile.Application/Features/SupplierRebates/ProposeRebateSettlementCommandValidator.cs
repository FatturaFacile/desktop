// <copyright file="ProposeRebateSettlementCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using FluentValidation;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Validator for <see cref="ProposeRebateSettlementCommand"/>: a VAT rate is needed unless the proposal sets off against a payable,
/// and a set-off needs the payable. The messages are localization keys.
/// </summary>
public sealed class ProposeRebateSettlementCommandValidator : AbstractValidator<ProposeRebateSettlementCommand>
{
    private const decimal MaxPercent = 100m;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProposeRebateSettlementCommandValidator"/> class.
    /// </summary>
    public ProposeRebateSettlementCommandValidator()
    {
        _ = RuleFor(x => x.AgreementId).NotEmpty().WithMessage(SupplierRebatesErrors.Required);
        _ = RuleFor(x => x.Input.TaxRate).NotNull().When(x => x.Input.Mode != RebateExecutionMode.ApSetOff).WithMessage(SupplierRebatesErrors.TaxRate);
        _ = RuleFor(x => x.Input.TaxRate).InclusiveBetween(0m, MaxPercent).When(x => x.Input.TaxRate is not null).WithMessage(SupplierRebatesErrors.TaxRate);
        _ = RuleFor(x => x.Input.PassiveInvoiceId).NotEmpty().When(x => x.Input.Mode == RebateExecutionMode.ApSetOff).WithMessage(SupplierRebatesErrors.InvoiceRequired);
    }
}
