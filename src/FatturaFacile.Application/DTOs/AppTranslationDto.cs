// <copyright file="AppTranslationDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of an application UI translation key and localized display value.
/// </summary>
/// <param name="Id">The unique identifier of the translation record.</param>
/// <param name="TenantId">The owning tenant identifier if this is a custom override; <see langword="null"/> for global system translations.</param>
/// <param name="Locale">The ISO language or culture identifier (e.g., "it", "de", "en", "fr").</param>
/// <param name="Key">The unique resource identifier key referenced in application templates or components.</param>
/// <param name="DefaultValue">The standard fallback localized text provided with the core distribution.</param>
/// <param name="CustomValue">The tenant-customized text override, if specified; otherwise <see langword="null"/>.</param>
/// <param name="EffectiveValue">The computed effective text value displayed to end users, taking customization priority into account.</param>
/// <param name="UpdatedAtUtc">The UTC timestamp when the translation entry was last modified.</param>
public record AppTranslationDto(
    Guid     Id,
    Guid?    TenantId,
    string   Locale,
    string   Key,
    string   DefaultValue,
    string?  CustomValue,
    string   EffectiveValue,
    DateTime UpdatedAtUtc);
