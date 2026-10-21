// <copyright file="UpdateTranslationCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using MediatR;

namespace FatturaFacile.Application.Features.Localization;

/// <summary>
/// Represents update translation command.
/// </summary>
/// <param name="Locale">The locale.</param>
/// <param name="Key">The key.</param>
/// <param name="CustomValue">The custom value.</param>
public record UpdateTranslationCommand(
    string Locale,
    string Key,
    string CustomValue) : IRequest<Unit>;
