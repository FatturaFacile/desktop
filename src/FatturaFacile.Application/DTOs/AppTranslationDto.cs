// <copyright file="AppTranslationDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Represents app translation dto.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="TenantId">The tenant id.</param>
/// <param name="Locale">The locale.</param>
/// <param name="Key">The key.</param>
/// <param name="DefaultValue">The default value.</param>
/// <param name="CustomValue">The custom value.</param>
/// <param name="EffectiveValue">The effective value.</param>
/// <param name="UpdatedAtUtc">The updated at utc.</param>
public record AppTranslationDto(
    Guid Id,
    Guid? TenantId,
    string Locale,
    string Key,
    string DefaultValue,
    string? CustomValue,
    string EffectiveValue,
    DateTime UpdatedAtUtc);
