// <copyright file="SaveDataRetentionSettingsCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.TimeTracking;
using FluentValidation;

namespace FatturaFacile.Application.Features.TimeTracking.Commands;

/// <summary>
/// Validates <see cref="SaveDataRetentionSettingsCommand"/>.
/// </summary>
public sealed class SaveDataRetentionSettingsCommandValidator : AbstractValidator<SaveDataRetentionSettingsCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SaveDataRetentionSettingsCommandValidator"/> class.
    /// </summary>
    public SaveDataRetentionSettingsCommandValidator()
    {
        _ = RuleFor(x => x.Years)
            .InclusiveBetween(DataRetentionPolicy.MinimumYears, DataRetentionPolicy.MaximumYears)
            .WithMessage("Le registrazioni vanno conservate per almeno 5 anni (art. 73 cpv. 2 OLL 1).");
    }
}
