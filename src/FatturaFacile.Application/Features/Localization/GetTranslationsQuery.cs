// <copyright file="GetTranslationsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Localization;

/// <summary>
/// Query to retrieve all localized application translation key-value mappings for a designated culture locale.
/// </summary>
/// <param name="Locale">The ISO language or culture identifier (e.g., "it", "de", "en", "fr").</param>
public record GetTranslationsQuery(string Locale) : IRequest<IReadOnlyList<AppTranslationDto>>;
