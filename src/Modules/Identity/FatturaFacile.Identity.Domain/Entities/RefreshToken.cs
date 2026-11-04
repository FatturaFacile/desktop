// <copyright file="RefreshToken.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Primitives;

namespace FatturaFacile.Identity.Domain.Entities;

/// <summary>
/// Represents a cryptographically secure refresh token for session continuity and rotation.
/// </summary>
public sealed class RefreshToken : Entity<Guid>
{
    private RefreshToken() { }

    /// <summary>
    /// Gets the unique identifier of the user who owns this refresh token.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the SHA-256 hash of the issued refresh token.
    /// </summary>
    public string TokenHash { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the UTC expiration timestamp for the refresh token.
    /// </summary>
    public DateTimeOffset ExpiresAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the refresh token was issued.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the refresh token was revoked, or null if active.
    /// </summary>
    public DateTimeOffset? RevokedAtUtc { get; private set; }

    /// <summary>
    /// Gets the replacement token hash if this token was rotated.
    /// </summary>
    public string? ReplacedByTokenHash { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the refresh token is currently valid and unrevoked.
    /// </summary>
    public bool IsActive => RevokedAtUtc == null && !IsExpired;

    /// <summary>
    /// Gets a value indicating whether the refresh token has passed its expiration timestamp.
    /// </summary>
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAtUtc;

    /// <summary>
    /// Factory method to create a new <see cref="RefreshToken"/> instance.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="tokenHash">The SHA-256 hash of the token.</param>
    /// <param name="lifetime">The validity duration of the token.</param>
    /// <returns>A new <see cref="RefreshToken"/> instance.</returns>
    public static RefreshToken Create(Guid userId, string tokenHash, TimeSpan lifetime)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = DateTimeOffset.UtcNow.Add(lifetime),
        };
    }

    /// <summary>
    /// Revokes the refresh token and records the optional replacement token hash.
    /// </summary>
    /// <param name="replacedByTokenHash">The hash of the newly rotated refresh token, if any.</param>
    public void Revoke(string? replacedByTokenHash = null)
    {
        RevokedAtUtc = DateTimeOffset.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
