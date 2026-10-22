// <copyright file="UpdateTranslationCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using MediatR;

namespace FatturaFacile.Application.Features.Localization;

/// <summary>
/// Command to update or override a localized text string for a specified culture locale and resource key.
/// </summary>
/// <param name="Locale">The ISO culture identifier code (e.g., "it", "de", "en", "fr").</param>
/// <param name="Key">The translation identifier key to override.</param>
/// <param name="CustomValue">The new customized translated text value.</param>
public record UpdateTranslationCommand(
    string Locale,
    string Key,
    string CustomValue) : IRequest<Unit>;
