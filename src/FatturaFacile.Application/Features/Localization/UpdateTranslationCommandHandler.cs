// <copyright file="UpdateTranslationCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using MediatR;

namespace FatturaFacile.Application.Features.Localization;

/// <summary>
/// Handles <see cref="UpdateTranslationCommand"/> by invoking the localization service to persist custom translation overrides.
/// </summary>
/// <param name="localizationService">The localization service used to persist custom translation strings.</param>
public sealed class UpdateTranslationCommandHandler(ILocalizationService localizationService)
    : IRequestHandler<UpdateTranslationCommand, Unit>
{
    private readonly ILocalizationService localizationService = localizationService;

    /// <summary>
    /// Processes the translation update command and saves the customized string value.
    /// </summary>
    /// <param name="request">The command containing locale, key, and override text value.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the update operation returning <see cref="Unit.Value"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    public async Task<Unit> Handle(
        UpdateTranslationCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await localizationService.SaveCustomTranslationAsync(
            request.Locale,
            request.Key,
            request.CustomValue,
            cancellationToken);
        return Unit.Value;
    }
}
