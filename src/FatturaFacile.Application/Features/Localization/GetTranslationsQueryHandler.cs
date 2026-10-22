// <copyright file="GetTranslationsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Localization;

/// <summary>
/// Handles <see cref="GetTranslationsQuery"/> by retrieving translation entries via <see cref="ILocalizationService"/>.
/// </summary>
/// <param name="localizationService">The localization service used to look up translation catalogues.</param>
public sealed class GetTranslationsQueryHandler(ILocalizationService localizationService)
    : IRequestHandler<GetTranslationsQuery, IReadOnlyList<AppTranslationDto>>
{
    private readonly ILocalizationService localizationService = localizationService;

    /// <summary>
    /// Processes the query, returning all default and customized translations for the specified locale.
    /// </summary>
    /// <param name="request">The query containing target locale specifications.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the query execution returning a read-only list of <see cref="AppTranslationDto"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
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
