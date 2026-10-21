// <copyright file="GetTranslationsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Localization;

/// <summary>
/// Represents get translations query.
/// </summary>
/// <param name="Locale">The locale.</param>
public record GetTranslationsQuery(string Locale) : IRequest<IReadOnlyList<AppTranslationDto>>;
