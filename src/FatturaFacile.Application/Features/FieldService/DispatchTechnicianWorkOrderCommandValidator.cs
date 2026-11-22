// <copyright file="DispatchTechnicianWorkOrderCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Validator for <see cref="DispatchTechnicianWorkOrderCommand"/>. The messages are localization keys, resolved by the view.
/// </summary>
public sealed class DispatchTechnicianWorkOrderCommandValidator : AbstractValidator<DispatchTechnicianWorkOrderCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DispatchTechnicianWorkOrderCommandValidator"/> class.
    /// </summary>
    public DispatchTechnicianWorkOrderCommandValidator()
    {
        _ = RuleFor(x => x.WorkOrderId).Must(BeIdentifier).WithMessage("FieldService_Err_WorkOrder");
        _ = RuleFor(x => x.TechnicianId).Must(BeIdentifier).WithMessage("FieldService_Err_Technician");
        _ = RuleFor(x => x.End).GreaterThan(x => x.Start).WithMessage("FieldService_Err_Window");
    }

    private static bool BeIdentifier(string value) => Guid.TryParse(value, out var id) && id != Guid.Empty;
}
