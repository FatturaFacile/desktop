// <copyright file="AdvanceRebateSettlementCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using FluentValidation;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Validator for <see cref="AdvanceRebateSettlementCommand"/>: the step must be allowed by the settlement state machine from the
/// status the user saw. The messages are localization keys.
/// </summary>
public sealed class AdvanceRebateSettlementCommandValidator : AbstractValidator<AdvanceRebateSettlementCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AdvanceRebateSettlementCommandValidator"/> class.
    /// </summary>
    public AdvanceRebateSettlementCommandValidator()
    {
        _ = RuleFor(x => x.AgreementId).NotEmpty().WithMessage(SupplierRebatesErrors.Required);
        _ = RuleFor(x => x.ProposalId).NotEmpty().WithMessage(SupplierRebatesErrors.Required);
        _ = RuleFor(x => x).Must(x => SettlementWorkflow.CanApply(x.CurrentStatus, x.Action)).WithMessage(SupplierRebatesErrors.ActionNotAllowed);
    }
}
