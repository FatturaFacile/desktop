namespace FatturaFacile.Application.DTOs;

public record AppTranslationDto(
    Guid Id,
    Guid? TenantId,
    string Locale,
    string Key,
    string DefaultValue,
    string? CustomValue,
    string EffectiveValue,
    DateTime UpdatedAtUtc);