// <copyright file="GetTranslationsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Localization;

/// <summary>
/// Represents get translations query handler.
/// </summary>
/// <param name="localizationService">The localization service.</param>
public sealed class GetTranslationsQueryHandler(ILocalizationService localizationService)
    : IRequestHandler<GetTranslationsQuery, IReadOnlyList<AppTranslationDto>>
{
    private readonly ILocalizationService localizationService = localizationService;

    /// <summary>
    /// Handle.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<IReadOnlyList<AppTranslationDto>> Handle(
        GetTranslationsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return localizationService.GetAllTranslationsAsync(
            request.Locale,
            cancellationToken);
    }
}
