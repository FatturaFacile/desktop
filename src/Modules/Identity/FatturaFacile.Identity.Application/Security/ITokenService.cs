// <copyright file="ITokenService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Identity.Domain.Aggregates;

namespace FatturaFacile.Identity.Application.Security;

/// <summary>
/// Service contract for generating access tokens and secure refresh tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a signed JWT bearer access token for an authenticated user.
    /// </summary>
    /// <param name="user">The user entity.</param>
    /// <param name="plainEmail">The user's plain email address.</param>
    /// <returns>A signed JWT token string.</returns>
    public string GenerateAccessToken(User user, string plainEmail);

    /// <summary>
    /// Generates a cryptographically random refresh token and its SHA-256 hash.
    /// </summary>
    /// <returns>A tuple containing the raw token and its database storage hash.</returns>
    public (string Token, string TokenHash) GenerateRefreshToken();

    /// <summary>
    /// Computes the SHA-256 hash of a raw refresh token string.
    /// </summary>
    /// <param name="token">The plaintext refresh token string.</param>
    /// <returns>The hex-encoded SHA-256 hash.</returns>
    public string HashRefreshToken(string token);
}
