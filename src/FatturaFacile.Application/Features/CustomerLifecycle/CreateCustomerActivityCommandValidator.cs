// <copyright file="CreateCustomerActivityCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.CustomerLifecycle;
using FluentValidation;

namespace FatturaFacile.Application.Features.CustomerLifecycle;

/// <summary>
/// Validator of <see cref="CreateCustomerActivityCommand"/>.
/// </summary>
public sealed class CreateCustomerActivityCommandValidator : AbstractValidator<CreateCustomerActivityCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCustomerActivityCommandValidator"/> class.
    /// </summary>
    public CreateCustomerActivityCommandValidator()
    {
        _ = RuleFor(x => x.AccountId).NotEmpty();
        _ = RuleFor(x => x.Type).Must(type => string.Equals(type, CustomerActivityTypes.Meeting, StringComparison.Ordinal) || string.Equals(type, CustomerActivityTypes.FollowUp, StringComparison.Ordinal));
        _ = RuleFor(x => x.Subject).NotEmpty().MaximumLength(CustomerActivityTypes.MaxSubjectLength);
    }
}
