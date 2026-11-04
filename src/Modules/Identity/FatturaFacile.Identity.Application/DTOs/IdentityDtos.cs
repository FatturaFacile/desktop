using FatturaFacile.Application.Shared.Contracts;

namespace FatturaFacile.Identity.Application.DTOs;

public sealed record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAtUtc,
    Guid UserId,
    Guid TenantId,
    string PreferredLanguage);

public sealed record TenantDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive);

public sealed record UserDto(
    Guid Id,
    Guid TenantId,
    string Email,
    string PreferredLanguage,
    bool IsActive);
