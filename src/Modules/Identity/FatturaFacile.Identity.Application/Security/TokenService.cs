using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using FatturaFacile.Identity.Domain.Aggregates;

namespace FatturaFacile.Identity.Application.Security;

/// <summary>
/// Configuration options for JSON Web Token issuance.
/// </summary>
public sealed class JwtOptions
{
    public string Issuer { get; set; } = "FatturaFacile";
    public string Audience { get; set; } = "FatturaFacile.Client";
    public string SecretKey { get; set; } = "FatturaFacile-UltraSecure-SuperSecret-SigningKey-2026-ModularERP";
    public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(60);
    public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(7);
}

/// <summary>
/// Service contract for generating access tokens and secure refresh tokens.
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(User user, string plainEmail);
    (string Token, string TokenHash) GenerateRefreshToken();
    string HashRefreshToken(string token);
}

public sealed class TokenService : ITokenService
{
    private readonly JwtOptions _options;

    public TokenService(JwtOptions? options = null)
    {
        _options = options ?? new JwtOptions();
    }

    public string GenerateAccessToken(User user, string plainEmail)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("tenant_id", user.TenantId.ToString()),
            new(JwtRegisteredClaimNames.Email, plainEmail),
            new("locale", user.PreferredLanguage),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(_options.AccessTokenLifetime),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string Token, string TokenHash) GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(randomBytes);
        var hash = HashRefreshToken(token);
        return (token, hash);
    }

    public string HashRefreshToken(string token)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes);
    }
}
