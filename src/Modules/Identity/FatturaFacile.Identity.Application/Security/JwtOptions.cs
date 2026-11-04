// <copyright file="JwtOptions.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Identity.Application.Security;

/// <summary>
/// Configuration options for JSON Web Token issuance.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// Gets or sets the token issuer name.
    /// </summary>
    public string Issuer { get; set; } = "FatturaFacile";

    /// <summary>
    /// Gets or sets the expected token audience.
    /// </summary>
    public string Audience { get; set; } = "FatturaFacile.Client";

    /// <summary>
    /// Gets or sets the symmetric signing key used to sign JWTs.
    /// </summary>
    public string SecretKey { get; set; } = "FatturaFacile-UltraSecure-SuperSecret-SigningKey-2026-ModularERP";

    /// <summary>
    /// Gets or sets the access token lifetime duration.
    /// </summary>
    public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(60);

    /// <summary>
    /// Gets or sets the refresh token validity duration.
    /// </summary>
    public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(7);
}
