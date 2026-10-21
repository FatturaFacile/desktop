// <copyright file="UpdateTranslationCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using MediatR;

namespace FatturaFacile.Application.Features.Localization;

/// <summary>
/// Represents update translation command handler.
/// </summary>
/// <param name="localizationService">The localization service.</param>
public sealed class UpdateTranslationCommandHandler(ILocalizationService localizationService)
    : IRequestHandler<UpdateTranslationCommand, Unit>
{
    private readonly ILocalizationService localizationService = localizationService;

    /// <summary>
    /// Handle.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
