// <copyright file="CreateContactCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.Contacts.Commands;

/// <summary>
/// Validator for <see cref="CreateContactCommand"/>.
/// </summary>
public sealed class CreateContactCommandValidator : AbstractValidator<CreateContactCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateContactCommandValidator"/> class.
    /// </summary>
    public CreateContactCommandValidator()
    {
        RuleFor(x => x.Country).NotEmpty().Length(2).WithMessage("Two-letter country code is required.");
        RuleFor(x => x).Must(HaveName).WithMessage("Either Company Name or First and Last Name must be provided.");
    }

    private static bool HaveName(CreateContactCommand cmd) => !string.IsNullOrWhiteSpace(cmd.CompanyName) || (!string.IsNullOrWhiteSpace(cmd.FirstName) && !string.IsNullOrWhiteSpace(cmd.LastName));
}
