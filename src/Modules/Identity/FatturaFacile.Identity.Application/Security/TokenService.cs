// <copyright file="TokenService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FatturaFacile.Identity.Domain.Aggregates;
using Microsoft.IdentityModel.Tokens;

namespace FatturaFacile.Identity.Application.Security;

/// <summary>
/// Default implementation of <see cref="ITokenService"/> providing HMAC-SHA256 JWTs and CSPRNG refresh tokens.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TokenService"/> class.
/// </remarks>
/// <param name="options">The JWT options.</param>
public sealed class TokenService(JwtOptions? options = null) : ITokenService
{
    private readonly JwtOptions _options = options ?? new JwtOptions();

    /// <inheritdoc />
    public string GenerateAccessToken(User user, string plainEmail)
    {
        ArgumentNullException.ThrowIfNull(user);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("tenant_id", user.TenantId.ToString()),
            new(JwtRegisteredClaimNames.Email, plainEmail),
            new("preferred_language", user.PreferredLanguage),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(_options.AccessTokenLifetime),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc />
    public (string Token, string TokenHash) GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(randomBytes);
        var tokenHash = HashRefreshToken(token);
        return (token, tokenHash);
    }

    /// <inheritdoc />
    public string HashRefreshToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
