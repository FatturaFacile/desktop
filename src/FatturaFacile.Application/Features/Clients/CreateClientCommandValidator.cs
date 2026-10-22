// <copyright file="CreateClientCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

using FluentValidation;

namespace FatturaFacile.Application.Features.Clients;

/// <summary>
/// Validator defining business and structural invariants for <see cref="CreateClientCommand"/> across Italian and Swiss jurisdictions.
/// </summary>
public sealed class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateClientCommandValidator"/> class, setting address, contact, and jurisdiction rules.
    /// </summary>
    public CreateClientCommandValidator()
    {
        _ = RuleFor(expression: x => x.BusinessName).NotEmpty().MaximumLength(maximumLength: 200);
        _ = RuleFor(expression: x => x.Email).NotEmpty().EmailAddress();
        _ = RuleFor(expression: x => x.Street).NotEmpty();
        _ = RuleFor(expression: x => x.City).NotEmpty();
        _ = RuleFor(expression: x => x.PostalCode).NotEmpty();

        _ = When(
                 predicate: x => x.Jurisdiction == Jurisdiction.Italy,
                 action: () =>
                         {
                             _ = RuleFor(expression: x => x.FiscalCode).NotEmpty();
                             _ = RuleFor(expression: x => x.VatNumber).NotEmpty();
                         });

        _ = When(
                 predicate: x => x.Jurisdiction == Jurisdiction.Switzerland,
                 action: () => _ = RuleFor(expression: x => x.SwissUid).NotEmpty());
    }

}
