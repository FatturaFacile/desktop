// <copyright file="AuthResponseDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Shared.Contracts;

namespace FatturaFacile.Identity.Application.DTOs;

/// <summary>
/// Data transfer object returned upon successful user authentication or token refresh.
/// </summary>
/// <param name="AccessToken">The short-lived signed JWT bearer token.</param>
/// <param name="RefreshToken">The opaque refresh token string for rotation.</param>
/// <param name="ExpiresAtUtc">The UTC timestamp when the access token expires.</param>
/// <param name="UserId">The authenticated user's unique identifier.</param>
/// <param name="TenantId">The active tenant identifier.</param>
/// <param name="PreferredLanguage">The user's preferred language code.</param>
public sealed record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAtUtc,
    Guid UserId,
    Guid TenantId,
    string PreferredLanguage);
