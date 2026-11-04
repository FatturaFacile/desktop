using FatturaFacile.Application.Shared.Contracts;
using FatturaFacile.Identity.Application.DTOs;

namespace FatturaFacile.Identity.Application.Commands;

/// <summary>
/// Command to authenticate an existing tenant user.
/// </summary>
public sealed record LoginUserCommand(
    string TenantSlug,
    string Email,
    string Password,
    string? IpAddress = null,
    string? UserAgent = null) : ICommand<AuthResponseDto>;

/// <summary>
/// Command to rotate an expired or expiring access token via refresh token.
/// </summary>
public sealed record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress = null) : ICommand<AuthResponseDto>;

/// <summary>
/// Command to register a new tenant and its initial administrator user.
/// </summary>
public sealed record RegisterTenantUserCommand(
    string TenantName,
    string TenantSlug,
    string Email,
    string Password,
    string PreferredLanguage = "en") : ICommand<UserDto>;
